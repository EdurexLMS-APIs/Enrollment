#pragma checksum "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "fd3925b8b193f32587dcc599e32172f4a444fc4a"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Shared__adminMenu), @"mvc.1.0.view", @"/Views/Shared/_adminMenu.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\_ViewImports.cshtml"
using CPEA;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\_ViewImports.cshtml"
using CPEA.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"fd3925b8b193f32587dcc599e32172f4a444fc4a", @"/Views/Shared/_adminMenu.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"368f9781feefc3edff7f3832f6ea5b7d8b8e9b03", @"/Views/_ViewImports.cshtml")]
    #nullable restore
    public class Views_Shared__adminMenu : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    #nullable disable
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\n<ul class=\"nav\">\n    <li class=\"nav-item\">\n        <a");
            BeginWriteAttribute("href", " href=\"", 13713, "\"", 13752, 1);
#nullable restore
#line 299 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 13720, Url.Action("Dashboard","Admin"), 13720, 32, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">\n            <i class=\"link-icon\" data-feather=\"box\"></i>\n            <span class=\"link-title\">Dashboard</span>\n        </a>\n    </li>\n    <li class=\"nav-item\">\n        <a");
            BeginWriteAttribute("href", " href=\"", 13942, "\"", 13984, 1);
#nullable restore
#line 305 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 13949, Url.Action("Institutions","Admin"), 13949, 35, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(@" class=""nav-link"">
            <i class=""link-icon"" data-feather=""box""></i>
            <span class=""link-title"">Institutions</span>
        </a>
    </li>
    <li class=""nav-item"">
        <a class=""nav-link"" data-toggle=""collapse"" href=""#programCourses"" role=""button"" aria-expanded=""false"" aria-controls=""programCourses"">
            <i class=""link-icon"" data-feather=""layout""></i>
            <span class=""link-title"">Program and Courses</span>
            <i class=""link-arrow"" data-feather=""chevron-down""></i>
        </a>
        <div class=""collapse"" id=""programCourses"">
            <ul class=""nav sub-menu"">
");
            WriteLiteral("                <li class=\"nav-item\">\n                    <a");
            BeginWriteAttribute("href", " href=\"", 14818, "\"", 14865, 1);
#nullable restore
#line 322 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 14825, Url.Action("ProgramCategories","Admin"), 14825, 40, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">Categories</a>\n                </li>\n                <li class=\"nav-item\">\n                    <a");
            BeginWriteAttribute("href", " href=\"", 14981, "\"", 15018, 1);
#nullable restore
#line 325 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 14988, Url.Action("Courses","Admin"), 14988, 30, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">Courses</a>\n                </li>\n                <li class=\"nav-item\">\n                    <a");
            BeginWriteAttribute("href", " href=\"", 15131, "\"", 15175, 1);
#nullable restore
#line 328 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 15138, Url.Action("Certifications","Admin"), 15138, 37, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(@" class=""nav-link"">Certifications</a>
                </li>
            </ul>
        </div>
    </li>
    <li class=""nav-item"">
        <a class=""nav-link"" data-toggle=""collapse"" href=""#studentManager"" role=""button"" aria-expanded=""false"" aria-controls=""studentManager"">
            <i class=""link-icon"" data-feather=""book-open""></i>
            <span class=""link-title"">Student Manager</span>
            <i class=""link-arrow"" data-feather=""chevron-down""></i>
        </a>
        <div class=""collapse"" id=""studentManager"">
            <ul class=""nav sub-menu"">
                <li class=""nav-item"">
                    <a");
            BeginWriteAttribute("href", " href=\"", 15798, "\"", 15846, 1);
#nullable restore
#line 342 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 15805, Url.Action("RegisteredStudents","Admin"), 15805, 41, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">All Students</a>\n                </li>\n                <li class=\"nav-item\">\n                    <a");
            BeginWriteAttribute("href", " href=\"", 15964, "\"", 16013, 1);
#nullable restore
#line 345 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 15971, Url.Action("CompletedEnrollment","Admin"), 15971, 42, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">Complete Enrollment</a>\n                </li>\n                <li class=\"nav-item\">\n                    <a");
            BeginWriteAttribute("href", " href=\"", 16138, "\"", 16185, 1);
#nullable restore
#line 348 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 16145, Url.Action("PartialEnrollment","Admin"), 16145, 40, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">Partial Enrollment</a>\n                </li>\n                <li class=\"nav-item\">\n                    <a");
            BeginWriteAttribute("href", " href=\"", 16309, "\"", 16354, 1);
#nullable restore
#line 351 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 16316, Url.Action("BlockedStudents","Admin"), 16316, 38, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(@" class=""nav-link"">Blocked Students</a>
                </li>
            </ul>
        </div>
    </li>
    <li class=""nav-item"">
        <a class=""nav-link"" data-toggle=""collapse"" href=""#userManager"" role=""button"" aria-expanded=""false"" aria-controls=""userManager"">
            <i class=""link-icon"" data-feather=""users""></i>
            <span class=""link-title"">User Manager</span>
            <i class=""link-arrow"" data-feather=""chevron-down""></i>
        </a>
        <div class=""collapse"" id=""userManager"">
            <ul class=""nav sub-menu"">
                <li class=""nav-item"">
                    <a");
            BeginWriteAttribute("href", " href=\"", 16963, "\"", 17008, 1);
#nullable restore
#line 365 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 16970, Url.Action("RegisteredUsers","Admin"), 16970, 38, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">All Users</a>\n                </li>\n                <li class=\"nav-item\">\n                    <a");
            BeginWriteAttribute("href", " href=\"", 17123, "\"", 17158, 1);
#nullable restore
#line 368 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 17130, Url.Action("Roles","Admin"), 17130, 28, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">User Roles</a>\n                </li>\n                <li class=\"nav-item\">\n                    <a");
            BeginWriteAttribute("href", " href=\"", 17274, "\"", 17316, 1);
#nullable restore
#line 371 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 17281, Url.Action("BlockedUsers","Admin"), 17281, 35, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">Blocked Users</a>\n                </li>\n                <li class=\"nav-item\">\n                    <a");
            BeginWriteAttribute("href", " href=\"", 17435, "\"", 17475, 1);
#nullable restore
#line 374 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 17442, Url.Action("Register2", "Admin"), 17442, 33, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">Register New User</a>\n                </li>\n");
            WriteLiteral("\n                                        </ul>\n        </div>\n    </li>\n    <li class=\"nav-item\">\n        <a");
            BeginWriteAttribute("href", " href=\"", 17927, "\"", 17977, 1);
#nullable restore
#line 387 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 17934, Url.Action("RegisteredAffiliates","Admin"), 17934, 43, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(@" class=""nav-link"">
            <i class=""link-icon"" data-feather=""box""></i>
            <span class=""link-title"">Affiliates</span>
        </a>
    </li>
    <li class=""nav-item"">
        <a class=""nav-link"" data-toggle=""collapse"" href=""#paymentRecord"" role=""button"" aria-expanded=""false"" aria-controls=""paymentRecord"">
            <i class=""link-icon"" data-feather=""layers""></i>
            <span class=""link-title"">Payment Record</span>
            <i class=""link-arrow"" data-feather=""chevron-down""></i>
        </a>
        <div class=""collapse"" id=""paymentRecord"">
            <ul class=""nav sub-menu"">
                <li class=""nav-item"">
                    <a");
            BeginWriteAttribute("href", " href=\"", 18645, "\"", 18683, 1);
#nullable restore
#line 401 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 18652, Url.Action("Payments","Admin"), 18652, 31, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">Payment History</a>\n                </li>\n                <li class=\"nav-item\">\n                    <a");
            BeginWriteAttribute("href", " href=\"", 18804, "\"", 18848, 1);
#nullable restore
#line 404 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 18811, Url.Action("TransactionLog","Admin"), 18811, 37, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">Transaction Log</a>\n                </li>\n            </ul>\n        </div>\n    </li>\n    <li class=\"nav-item\">\n        <a");
            BeginWriteAttribute("href", " href=\"", 18988, "\"", 19024, 1);
#nullable restore
#line 410 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 18995, Url.Action("Report","Admin"), 18995, 29, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">\n            <i class=\"link-icon\" data-feather=\"list\"></i>\n            <span class=\"link-title\">Report</span>\n        </a>\n    </li>\n    <li class=\"nav-item\">\n        <a");
            BeginWriteAttribute("href", " href=\"", 19212, "\"", 19248, 1);
#nullable restore
#line 416 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Shared\_adminMenu.cshtml"
WriteAttributeValue("", 19219, Url.Action("Logout","Admin"), 19219, 29, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" class=\"nav-link\">\n            <i class=\"link-icon\" data-feather=\"log-out\"></i>\n            <span class=\"link-title\">Log Out</span>\n        </a>\n    </li>\n\n</ul>\n\n    ");
        }
        #pragma warning restore 1998
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; } = default!;
        #nullable disable
        #nullable restore
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; } = default!;
        #nullable disable
    }
}
#pragma warning restore 1591
