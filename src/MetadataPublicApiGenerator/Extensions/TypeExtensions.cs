// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

            type.Attributes.TryGetNullable(out var nullable);

            if (baseEntity != null && baseEntity.KnownType != KnownTypeCode.Object)
            {
                bases.Add(SimpleBaseType(baseEntity.GetTypeSyntax(type, nullableContext, nullable)));
            }

            bases.AddRange(type.GetInterfaceBaseTypes(nullableContext, nullable));

            return bases;
        }

        public static IReadOnlyCollection<BaseTypeSyntax> GetInterfaceBaseTypes(this TypeWrapper type, Nullability nullableContext, Nullability[] nullable)
        {
            var interfaces = type.InterfaceImplementations;

            var bases = new BaseTypeSyntax[interfaces.Count];

            int i = 0;
            foreach (var interfaceType in interfaces)
            {
                bases[i] = SimpleBaseType(interfaceType.GetTypeSyntax(type, nullableContext, nullable));
                i++;
            }

            return bases;
        }

        public static TypeSyntax GetTypeSyntax(this IHandleTypeNamedWrapper wrapper, IHandleNameWrapper? parent, Nullability nullableContext, Nullability[] nullable, bool includeRef = true, int nullableIndex = 0)
        {
            if (wrapper is ArrayTypeWrapper arrayType)
            {
                return GenerateArrayType(arrayType, nullableContext, nullable, includeRef, nullableIndex);
            }

            if ((wrapper is IHasTypeArguments parameterizedTypeWrapper) && parameterizedTypeWrapper.TypeArguments.Count > 0)
            {
                if (wrapper.FullName.StartsWith("System.ValueTuple", StringComparison.InvariantCulture) && parent != null)
                {
                    return GenerateValueTuple(parameterizedTypeWrapper, parent!, nullableContext, nullable, includeRef, nullableIndex);
                }

                return GenerateTypeArgumentsType(wrapper, parent, nullableContext, nullable, includeRef, parameterizedTypeWrapper, nullableIndex);
            }

            if ((wrapper is IHasGenericParameters generics) && generics.GenericParameters.Count > 0)
            {
                return GenerateGenericParameter(wrapper, parent, nullableContext, nullable, includeRef, generics, nullableIndex);
            }

            if (includeRef && wrapper is ByReferenceWrapper byRefWrapper)
            {
                return RefType(GetTypeSyntax(byRefWrapper.EnclosedType, parent, nullableContext, nullable, true, nullableIndex), false);
            }

            if (wrapper is PointerWrapper pointerWrapper)
            {
                return PointerType(GetTypeSyntax(pointerWrapper.EnclosedType, parent, nullableContext, nullable, includeRef, nullableIndex));
            }

            return CreateNonGenericTypeSyntax(wrapper, nullableContext, nullable, nullableIndex);
        }

        public static (IReadOnlyCollection<TypeParameterConstraintClauseSyntax> TypeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> TypeParameters) GetTypeParameters(this IHasGenericParameters genericParameterContainer, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Nullability nullableContext)
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

                foreach (var constraint in genericParameter.Constraints)
                {
                    if (constraint.Type.ReflectionFullName.Equals("System.ValueType", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    constraints.Add(TypeConstraint(constraint.Type.GetTypeSyntax(null, nullableContext, EmptyNullability)));
                }

                if (genericParameter.HasDefaultConstructorConstraint)
                {
                    constraints.Add(ConstructorConstraint());
                }

                if (constraints.Count > 0)
                {
                    constraintClauses.Add(TypeParameterConstraintClause(genericParameter.Name, constraints));
                }
            }

            var parameters = new List<TypeParameterSyntax>(genericParameterContainer.GenericParameters.Count);
            foreach (var genericParameter in genericParameterContainer.GenericParameters)
            {
                var parameter = TypeParameterSymbolGenerator.Generate(genericParameter, excludeMembersAttributes, excludeAttributes);

                if (parameter == null)
                {
                    continue;
                }

                parameters.Add(parameter);
            }

            return (constraintClauses, parameters);
        }

        private static TypeSyntax GenerateArrayType(ArrayTypeWrapper arrayType, Nullability nullableContext, Nullability[] nullable, bool includeRef, int nullableIndex)
        {
            var arraySpecifiers = new List<ArrayRankSpecifierSyntax>(16);

            var current = arrayType;

            while (current.EnclosedType is ArrayTypeWrapper child)
            {
                arraySpecifiers.Add(GetArrayRankSpecifierSyntax(current));
                current = child;
            }

            arraySpecifiers.Add(GetArrayRankSpecifierSyntax(current));

            var elementNullability = nullable != null && nullableIndex < nullable.Length ? nullable[nullableIndex] : nullableContext;
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

        private static TypeSyntax GenerateGenericParameter(IHandleTypeNamedWrapper wrapper, IHandleNameWrapper? parent, Nullability nullableContext, Nullability[] nullable, bool includeRef, IHasGenericParameters generics, int nullableIndex)
        {
            var typeArguments = generics.GenericParameters.Select(x => GetTypeSyntax(x, parent, nullableContext, nullable, includeRef, nullableIndex)).ToList();

            return CreateGenericTypeSyntax(wrapper, typeArguments, nullableContext, nullable, nullableIndex);
        }

        private static TypeSyntax GenerateValueTuple(IHasTypeArguments type, IHandleNameWrapper parent, Nullability nullableContext, Nullability[] nullable, bool includeRef, int nullableIndex)
        {
            parent.HasTupleElementNamesAttribute(out var names);

            var namesLength = names?.Length ?? 0;

            var valueTupleElements = new List<TupleElementSyntax>();
            for (int i = 0; i < type.TypeArguments.Count; ++i)
            {
                int childNullableIndex = nullableIndex + i + 1;
                var genericParameter = type.TypeArguments[i].GetTypeSyntax(parent, nullableContext, nullable, includeRef, childNullableIndex);
                string? name = names != null && i < namesLength ? names[i] : null;

                valueTupleElements.Add(TupleElement(genericParameter, name));
            }

            return TupleType(valueTupleElements);
        }

        private static TypeSyntax GenerateTypeArgumentsType(IHandleTypeNamedWrapper wrapper, IHandleNameWrapper? parent, Nullability nullableContext, Nullability[] nullable, bool includeRef, IHasTypeArguments parameterizedTypeWrapper, int nullableIndex)
        {
            var typeArguments = new TypeSyntax[parameterizedTypeWrapper.TypeArguments.Count];
            for (int i = 0; i < parameterizedTypeWrapper.TypeArguments.Count; ++i)
            {
                int childNullableIndex = nullableIndex + i + 1;
                typeArguments[i] = GetTypeSyntax(parameterizedTypeWrapper.TypeArguments[i], parent, nullableContext, nullable, includeRef, childNullableIndex);
            }

            return CreateGenericTypeSyntax(wrapper, typeArguments, nullableContext, nullable, nullableIndex);
        }

        private static TypeSyntax CreateNonGenericTypeSyntax(IHandleTypeNamedWrapper wrapper, Nullability nullableContext, Nullability[] nullable, int nullableIndex)
        {
            if (wrapper == null)
            {
                throw new ArgumentNullException(nameof(wrapper));
            }

            var type = IdentifierName(wrapper.ReflectionFullName);
            return CreateTypeSyntax(wrapper, type, nullableContext, nullable, nullableIndex);
        }

        private static TypeSyntax CreateGenericTypeSyntax(IHandleTypeNamedWrapper wrapper, IReadOnlyCollection<TypeSyntax> typeArguments, Nullability nullableContext, Nullability[] nullable, int nullableIndex)
        {
            if (wrapper == null)
            {
                throw new ArgumentNullException(nameof(wrapper));
            }

            if (typeArguments == null)
            {
                throw new ArgumentNullException(nameof(typeArguments));
            }

            if (typeArguments.Any(x => x == null))
            {
                throw new ArgumentException("Type arguments has a invalid list member.", nameof(typeArguments));
            }

            var type = GenericName(wrapper.ReflectionFullName, typeArguments);
            return CreateTypeSyntax(wrapper, type, nullableContext, nullable, nullableIndex);
        }

        private static TypeSyntax CreateTypeSyntax(IHandleTypeNamedWrapper wrapper, TypeSyntax typeSyntax, Nullability nullableContext, Nullability[] nullable, int nullableIndex)
        {
            var nullability = nullable == null || nullableIndex >= nullable.Length ? nullableContext : nullable[nullableIndex];

            if (nullability == Nullability.Nullable && !wrapper.IsValueType)
            {
                return NullableType(typeSyntax);
            }

            return typeSyntax;
        }
    }
}
