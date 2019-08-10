using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MetadataPublicApiGenerator.IntegrationTestData
{
    public class NullableContext
    {
#nullable enable
        public void WithoutNullInsideContext(string input) { }

        public void WithNullInsideContext(string? input) { }
#nullable restore

#nullable disable
        public void WithoutNullInsideDisable(string input) { }
        public void WithNullInsideDisable(string? input) { }
#nullable restore
    }
}
