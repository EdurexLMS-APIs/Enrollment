#pragma checksum "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "94d6aa78539aaaf5484555f74d696b770e2e7b20"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Admin_InstitutionDestOffList), @"mvc.1.0.view", @"/Views/Admin/InstitutionDestOffList.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"94d6aa78539aaaf5484555f74d696b770e2e7b20", @"/Views/Admin/InstitutionDestOffList.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"368f9781feefc3edff7f3832f6ea5b7d8b8e9b03", @"/Views/_ViewImports.cshtml")]
    #nullable restore
    public class Views_Admin_InstitutionDestOffList : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<CPEA.Utilities.DTO.InstitutionDestOffVM>
    #nullable disable
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("name", "EditInstitutionDestOff", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("name", "AddInstitutionDestOff", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        #pragma warning restore 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.PartialTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_PartialTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 1 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
  
    Layout = "~/Views/Shared/_adminLayout.cshtml";

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
            WriteLiteral(@"
    <style>
        /* The container */
        .container {
            display: block;
            position: relative;
            padding-left: 25px;
            margin-bottom: 10px;
            cursor: pointer;
            /*font-size: 22px;*/
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            user-select: none;
            font-weight: 400;
            font-size: 0.875rem;
        }

            /* Hide the browser's default checkbox */
            .container input {
                position: absolute;
                opacity: 0;
                cursor: pointer;
                height: 0;
                width: 0;
            }

        /* Create a custom checkbox */
        .checkmark {
            position: absolute;
            top: 0;
            left: 0;
            height: 20px;
            width: 20px;
            background-color: #eee;
        }

        /* On mouse-over, add a grey backg");
            WriteLiteral(@"round color */
        .container:hover input ~ .checkmark {
            background-color: #ccc;
        }

        /* When the checkbox is checked, add a blue background */
        .container input:checked ~ .checkmark {
            background-color: #2196F3;
        }

        /* Create the checkmark/indicator (hidden when not checked) */
        .checkmark:after {
            content: """";
            position: absolute;
            display: none;
        }

        /* Show the checkmark when checked */
        .container input:checked ~ .checkmark:after {
            display: block;
        }

        /* Style the checkmark/indicator */
        .container .checkmark:after {
            left: 9px;
            top: 5px;
            width: 5px;
            height: 10px;
            border: solid white;
            border-width: 0 3px 3px 0;
            -webkit-transform: rotate(45deg);
            -ms-transform: rotate(45deg);
            transform: rotate(45deg);
        }
 ");
            WriteLiteral("   </style>\r\n\r\n<nav class=\"page-breadcrumb\">\r\n    <ol class=\"breadcrumb\">\r\n        <li class=\"breadcrumb-item\"><a");
            BeginWriteAttribute("href", " href=\"", 2270, "\"", 2312, 1);
#nullable restore
#line 81 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
WriteAttributeValue("", 2277, Url.Action("Institutions","Admin"), 2277, 35, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(@">Institutions</a></li>
        <li class=""breadcrumb-item active"" aria-current=""page"">Desk Officers</li>
    </ol>
</nav>
<div class=""row"">
    <div class=""col-md-12"">
        <div class=""card"">
            <div class=""card-body"">
                <div class=""container-fluid mt-5 d-flex justify-content-center w-100"">
                    <div class=""table-responsive w-100"">
");
            WriteLiteral(@"                        <br />
                        <table class=""table table-hover mb-0 "" id=""dataTableExample"">
                            <thead>
                                <tr>
                                    <th class=""pt-0"">Title</th>
                                    <th class=""pt-0"">Full Name</th>
                                    <th class=""pt-0"">Email</th>
                                    <th class=""pt-0"">Phone Number</th>
                                    <th class=""pt-0"">Can Login</th>
                                    <th class=""pt-0"">Action</th>
                                </tr>
                            </thead>
                            <tbody>
");
#nullable restore
#line 107 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                 if (Model != null && Model.institutionDestO.Count > 0)
                                {
                                    

#line default
#line hidden
#nullable disable
#nullable restore
#line 109 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                     foreach (var item in Model.institutionDestO)
                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                        <tr>\r\n                                            <td>");
#nullable restore
#line 112 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                           Write(Html.DisplayFor(modelItem => item.Title));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                            <td>");
#nullable restore
#line 113 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                           Write(Html.DisplayFor(modelItem => item.FirstName));

#line default
#line hidden
#nullable disable
            WriteLiteral(" ");
#nullable restore
#line 113 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                                                                         Write(Html.DisplayFor(modelItem => item.LastName));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                            <td>");
#nullable restore
#line 114 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                           Write(Html.DisplayFor(modelItem => item.Email));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                            <td>");
#nullable restore
#line 115 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                           Write(Html.DisplayFor(modelItem => item.Phone));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                            <td>\r\n");
#nullable restore
#line 117 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                                 if (item.CanLogin == true)
                                                {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                    <span>True</span>\r\n");
#nullable restore
#line 120 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                                }
                                                else
                                                {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                    <span>False</span>\r\n");
#nullable restore
#line 124 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                                }

#line default
#line hidden
#nullable disable
            WriteLiteral("                                            </td>\r\n                                            <td>\r\n                                                ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("partial", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "94d6aa78539aaaf5484555f74d696b770e2e7b2011540", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_PartialTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.PartialTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_PartialTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_PartialTagHelper.Name = (string)__tagHelperAttribute_0.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_0);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral(@"

                                                <div class=""dropdown mb-2"">
                                                    <button class=""btn p-0"" type=""button"" id=""dropdownMenuButton"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"">
                                                        <i class=""icon-lg text-muted pb-3px"" data-feather=""more-horizontal""></i>
                                                    </button>
                                                    <div class=""dropdown-menu"" aria-labelledby=""dropdownMenuButton"">
                                                        <a class=""dropdown-item d-flex align-items-center"" href=""#""");
            BeginWriteAttribute("onclick", " onclick=\"", 5702, "\"", 5817, 18);
            WriteAttributeValue("", 5712, "Deskoffdit(", 5712, 11, true);
#nullable restore
#line 134 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
WriteAttributeValue("", 5723, item.Id, 5723, 8, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 5731, ",", 5731, 1, true);
            WriteAttributeValue(" ", 5732, "\'", 5733, 2, true);
#nullable restore
#line 134 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
WriteAttributeValue("", 5734, item.Title, 5734, 11, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 5745, "\',", 5745, 2, true);
            WriteAttributeValue(" ", 5747, "\'", 5748, 2, true);
#nullable restore
#line 134 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
WriteAttributeValue("", 5749, item.FirstName, 5749, 15, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 5764, "\',", 5764, 2, true);
            WriteAttributeValue(" ", 5766, "\'", 5767, 2, true);
#nullable restore
#line 134 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
WriteAttributeValue("", 5768, item.LastName, 5768, 14, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 5782, "\',", 5782, 2, true);
            WriteAttributeValue(" ", 5784, "\'", 5785, 2, true);
#nullable restore
#line 134 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
WriteAttributeValue("", 5786, item.Phone, 5786, 11, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 5797, "\',", 5797, 2, true);
            WriteAttributeValue(" ", 5799, "\'", 5800, 2, true);
#nullable restore
#line 134 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
WriteAttributeValue("", 5801, item.CanLogin, 5801, 14, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 5815, "\')", 5815, 2, true);
            EndWriteAttribute();
            WriteLiteral(" data-toggle=\"modal\" data-target=\"#deskOfficerEdit\"><i data-feather=\"edit\" class=\"icon-sm mr-2\"></i> <span");
            BeginWriteAttribute("class", " class=\"", 5924, "\"", 5932, 0);
            EndWriteAttribute();
            WriteLiteral(">Edit</span></a>\r\n                                                        <a class=\"dropdown-item d-flex align-items-center\"");
            BeginWriteAttribute("onclick", " onclick=\"", 6057, "\"", 6090, 3);
            WriteAttributeValue("", 6067, "DeleteOfficer(", 6067, 14, true);
#nullable restore
#line 135 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
WriteAttributeValue("", 6081, item.Id, 6081, 8, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 6089, ")", 6089, 1, true);
            EndWriteAttribute();
            WriteLiteral(@"><i data-feather=""trash"" class=""icon-sm mr-2""></i><span>Delete</span></a>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
");
#nullable restore
#line 140 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                    }

#line default
#line hidden
#nullable disable
#nullable restore
#line 140 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                     
                                }

#line default
#line hidden
#nullable disable
            WriteLiteral("                            </tbody>\r\n                        </table>\r\n                    </div>\r\n\r\n                </div>\r\n                <div>\r\n                    <a class=\"btn btn-primary\"");
            BeginWriteAttribute("onclick", " onclick=\"", 6649, "\"", 6691, 3);
            WriteAttributeValue("", 6659, "CourseData(", 6659, 11, true);
#nullable restore
#line 148 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
WriteAttributeValue("", 6670, Model.InstitutionId, 6670, 20, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 6690, ")", 6690, 1, true);
            EndWriteAttribute();
            WriteLiteral(" data-toggle=\"modal\" data-target=\"#deskOfficerAdd\" style=\"margin-top: 15px; background-color: #058383 !important \"> New Desk Officer</a>\r\n                    ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("partial", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "94d6aa78539aaaf5484555f74d696b770e2e7b2018234", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_PartialTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.PartialTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_PartialTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_PartialTagHelper.Name = (string)__tagHelperAttribute_1.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_1);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </div>\r\n</div>\r\n\r\n<script>\r\n\r\n    function DeleteOfficer(id) {\r\n        if (confirm(\"Do you want to delete this desk-officer?\")) {\r\n             window.location.href = \'");
#nullable restore
#line 160 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\InstitutionDestOffList.cshtml"
                                Write(Url.Action("DeleteInstitutionDestOff", "Admin"));

#line default
#line hidden
#nullable disable
            WriteLiteral(@"/' + id;
        }

    }
    function CourseData(g) {
        document.getElementById('InstitutionIdAdd').value = g;
    }

    function Deskoffdit(g, h, i, j, k,l) {
        
        document.getElementById('DeskOffId').value = g;
        document.getElementById('titleEdit').value = h;
        document.getElementById('firstNameEdit').value = i;
        document.getElementById('lastnameEdit').value = j;
        document.getElementById('phoneEdit').value = k;

        if (l == ""True"") {
            document.getElementById('CanLoginEdit').checked = true;
        }
    }
</script>


");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<CPEA.Utilities.DTO.InstitutionDestOffVM> Html { get; private set; } = default!;
        #nullable disable
    }
}
#pragma warning restore 1591
