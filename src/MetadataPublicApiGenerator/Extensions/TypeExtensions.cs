// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightweightMetadata;
using MetadataPublicApiGenerator.Generators.SymbolGenerators;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Extensions
{
    /// <summary>
    /// Gets static methods that will generate TypeSyntax.
    /// </summary>
    internal static class TypeExtensions
    {
        private static readonly Nullability[] EmptyNullability = Array.Empty<Nullability>();

        public static IReadOnlyCollection<BaseTypeSyntax> GetBaseTypes(this TypeWrapper type, Nullability nullableContext)
        {
            var baseEntity = type.Base;
            var interfaces = type.InterfaceImplementations;

            var bases = new List<BaseTypeSyntax>(1 + interfaces.Count);

            if (baseEntity != null && baseEntity.KnownType != KnownTypeCode.Object)
            {
                type.Attributes.TryGetNullable(out var nullable);
                bases.Add(SimpleBaseType(baseEntity.GetTypeSyntax(type, nullableContext, nullable)));
            }

            bases.AddRange(type.GetInterfaceBaseTypes(nullableContext));

            return bases;
        }

        public static IReadOnlyCollection<BaseTypeSyntax> GetInterfaceBaseTypes(this TypeWrapper type, Nullability nullableContext)
        {
            var interfaces = type.InterfaceImplementations;

            var bases = new BaseTypeSyntax[interfaces.Count];

            int i = 0;
            foreach (var interfaceType in interfaces)
            {
                interfaceType.InterfaceAttributes.TryGetNullable(out var nullability);

                bases[i] = SimpleBaseType(interfaceType.GetTypeSyntax(type, nullableContext, nullability));
                i++;
            }

            return bases;
        }

        public static TypeSyntax GetTypeSyntax(this IHandleTypeNamedWrapper wrapper, IHandleNameWrapper parent, Nullability nullableContext, Nullability[] nullable, bool includeRef = true)
        {
            if (wrapper is ArrayTypeWrapper arrayType)
            {
                return GenerateArrayType(arrayType, nullableContext, nullable, includeRef);
            }

            if ((wrapper is IHasTypeArguments parameterizedTypeWrapper) && parameterizedTypeWrapper.TypeArguments.Count > 0)
            {
                if (wrapper.FullName.StartsWith("System.ValueTuple", StringComparison.InvariantCulture))
                {
                    return GenerateValueTuple(parameterizedTypeWrapper, parent, nullableContext, nullable, includeRef);
                }

                return GenerateTypeArgumentsType(wrapper, parent, nullableContext, nullable, includeRef, parameterizedTypeWrapper);
            }

            if ((wrapper is IHasGenericParameters generics) && generics.GenericParameters.Count > 0)
            {
                return GenerateGenericParameter(wrapper, parent, nullableContext, nullable, includeRef, generics);
            }

            if (includeRef && wrapper is ByReferenceWrapper byRefWrapper)
            {
                return RefType(GetTypeSyntax(byRefWrapper, parent, nullableContext, nullable, includeRef), false);
            }

            if (wrapper is PointerWrapper pointerWrapper)
            {
                return PointerType(GetTypeSyntax(pointerWrapper.EnclosedType, parent, nullableContext, nullable, includeRef));
            }

            if (wrapper is TypeWrapper typeWrapper)
            {
                nullableContext = nullable == null || nullable.Length == 0 ? nullableContext : nullable[0];

                return CreateTypeSyntax(typeWrapper, nullableContext);
            }

            return CreateTypeSyntax(wrapper, nullableContext);
        }

        public static (IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters) GetTypeParameters(this IHasGenericParameters genericParameterContainer, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Nullability nullableContext)
        {
            if (genericParameterContainer.GenericParameters.Count == 0)
            {
                return default;
            }

            var constraintClauses = new List<TypeParameterConstraintClauseSyntax>(genericParameterContainer.GenericParameters.Count * 2);

            foreach (var genericParameter in genericParameterContainer.GenericParameters)
            {
                var constraints = new List<TypeParameterConstraintSyntax>(genericParameter.Constraints.Count + 3);

                if (genericParameter.HasReferenceTypeConstraint)
                {
                    constraints.Add(ClassOrStructConstraint(SyntaxKind.ClassConstraint));
                }

                if (genericParameter.HasValueTypeConstraint)
                {
                    constraints.Add(ClassOrStructConstraint(SyntaxKind.StructConstraint));
                }

                constraints.AddRange(genericParameter.Constraints.Where(x => !x.Type.ReflectionFullName.Equals("System.ValueType", StringComparison.InvariantCultureIgnoreCase)).Select(x => TypeConstraint(x.Type.GetTypeSyntax(null, nullableContext, EmptyNullability))));

                if (genericParameter.HasDefaultConstructorConstraint)
                {
                    constraints.Add(ConstructorConstraint());
                }

                if (constraints.Count > 0)
                {
                    constraintClauses.Add(TypeParameterConstraintClause(genericParameter.Name, constraints));
                }
            }

            var parameters = genericParameterContainer.GenericParameters.Select(x => TypeParameterSymbolGenerator.Generate(x, excludeMembersAttributes, excludeAttributes)).ToList();

            return (constraintClauses, parameters);
        }

        private static TypeSyntax GenerateArrayType(ArrayTypeWrapper arrayType, Nullability nullableContext, Nullability[] nullable, bool includeRef)
        {
            var arraySpecifiers = new List<ArrayRankSpecifierSyntax>(16);

            var current = arrayType;

            while (current.EnclosedType is ArrayTypeWrapper child)
            {
                arraySpecifiers.Add(GetArrayRankSpecifierSyntax(current));
                current = child;
            }

            arraySpecifiers.Add(GetArrayRankSpecifierSyntax(current));

            var elementNullability = nullable != null && nullable.Length > 1 ? nullable[1] : nullableContext;
            var arrayTypeSyntax = ArrayType(current.EnclosedType.GetTypeSyntax(arrayType, elementNullability, EmptyNullability, includeRef), arraySpecifiers);
            return nullableContext == Nullability.Nullable ? (TypeSyntax)NullableType(arrayTypeSyntax) : arrayTypeSyntax;
        }

        private static ArrayRankSpecifierSyntax GetArrayRankSpecifierSyntax(ArrayTypeWrapper current)
        {
            var shapeExpressions = new List<int?>(current.ArrayShapeData?.Rank ?? 0);

            if (current.ArrayShapeData != null)
            {
                for (int i = 0; i < current.ArrayShapeData.Rank; ++i)
                {
                    int? size = current.ArrayShapeData.Sizes.Count > 0 ? new int?(current.ArrayShapeData.Sizes[i]) : null;

                    shapeExpressions.Add(size);
                }
            }

            return ArrayRankSpecifier(shapeExpressions);
        }

        private static TypeSyntax GenerateGenericParameter(IHandleTypeNamedWrapper wrapper, IHandleNameWrapper parent, Nullability nullableContext, Nullability[] nullable, bool includeRef, IHasGenericParameters generics)
        {
            var typeArguments = generics.GenericParameters.Select(x => GetTypeSyntax(x, parent, nullableContext, EmptyNullability, includeRef)).ToList();

            return CreateTypeSyntax(wrapper, typeArguments, nullableContext);
        }

        private static TypeSyntax GenerateValueTuple(IHasTypeArguments type, IHandleNameWrapper parent, Nullability nullableContext, Nullability[] nullable, bool includeRef)
        {
            parent.HasTupleElementNamesAttribute(out var names);

            var namesLength = names?.Length ?? 0;

            var valueTupleElements = new List<TupleElementSyntax>();
            for (int i = 0; i < type.TypeArguments.Count; ++i)
            {
                int nullableIndex = i + 1;
                var nullability = nullableIndex >= nullable.Length ? nullableContext : nullable[nullableIndex];
                var genericParameter = type.TypeArguments[i].GetTypeSyntax(parent, nullability, EmptyNullability, includeRef);
                var name = names != null && i < namesLength ? names[i] : null;

                valueTupleElements.Add(TupleElement(genericParameter, name));
            }

            return TupleType(valueTupleElements);
        }

        private static TypeSyntax GenerateTypeArgumentsType(IHandleTypeNamedWrapper wrapper, IHandleNameWrapper parent, Nullability nullableContext, Nullability[] nullable, bool includeRef, IHasTypeArguments parameterizedTypeWrapper)
        {
            nullableContext = nullable.Length >= 1 ? nullable[0] : nullableContext;

            var typeArguments = new TypeSyntax[parameterizedTypeWrapper.TypeArguments.Count];
            for (int i = 0; i < parameterizedTypeWrapper.TypeArguments.Count; ++i)
            {
                int nullableIndex = i + 1;
                typeArguments[i] = GetTypeArgument(parameterizedTypeWrapper.TypeArguments[i], parent, nullableContext, nullable, includeRef, nullableIndex);
            }

            return CreateTypeSyntax(wrapper, typeArguments, nullableContext);
        }

        private static TypeSyntax GetTypeArgument(IHandleTypeNamedWrapper typeArgument, IHandleNameWrapper parent, Nullability nullableContext, Nullability[] nullable, bool includeRef, int nullableIndex)
        {
            var nullability = nullableIndex >= nullable.Length ? nullableContext : nullable[nullableIndex];
            if (typeArgument is IHasTypeArguments child)
            {
                var childItems = new TypeSyntax[child.TypeArguments.Count];
                for (int i = 0; i < child.TypeArguments.Count; ++i)
                {
                    var childItem = child.TypeArguments[i];
                    var childNullableIndex = nullableIndex + i + 1;
                    childItems[i] = GetTypeArgument(childItem, typeArgument, nullability, nullable, includeRef, childNullableIndex);
                }

                return CreateTypeSyntax(typeArgument, childItems, nullability);
            }

            return GetTypeSyntax(typeArgument, parent, nullability, EmptyNullability, includeRef);
        }

        private static TypeSyntax CreateTypeSyntax(IHandleTypeNamedWrapper wrapper, Nullability nullableContext)
        {
            var type = IdentifierName(wrapper.ReflectionFullName);
            return CreateTypeSyntax(wrapper, type, nullableContext);
        }

        private static TypeSyntax CreateTypeSyntax(IHandleTypeNamedWrapper wrapper, IReadOnlyCollection<TypeSyntax> typeArguments, Nullability nullableContext)
        {
            var type = GenericName(wrapper.ReflectionFullName, typeArguments);
            return CreateTypeSyntax(wrapper, type, nullableContext);
        }

        private static TypeSyntax CreateTypeSyntax(IHandleTypeNamedWrapper wrapper, TypeSyntax typeSyntax, Nullability nullableContext)
        {
            if (nullableContext == Nullability.Nullable && !wrapper.IsValueType)
            {
                return NullableType(typeSyntax);
            }

            return typeSyntax;
        }
    }
}
