#pragma checksum "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Home\GetProfile.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "bded577a68d591b4b6eefed452154f2436487346"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_GetProfile), @"mvc.1.0.view", @"/Views/Home/GetProfile.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"bded577a68d591b4b6eefed452154f2436487346", @"/Views/Home/GetProfile.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"368f9781feefc3edff7f3832f6ea5b7d8b8e9b03", @"/Views/_ViewImports.cshtml")]
    #nullable restore
    public class Views_Home_GetProfile : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<CPEA.Utilities.ViewModel.UserProfileVM>
    #nullable disable
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("profile-pic"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/assets/images/Default.jpg"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("width", new global::Microsoft.AspNetCore.Html.HtmlString("120"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_3 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("height", new global::Microsoft.AspNetCore.Html.HtmlString("120"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_4 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("alt", new global::Microsoft.AspNetCore.Html.HtmlString("profile"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
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
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral(@"
<div class=""profile-page tx-13"">
    <div class=""row"">
        <div class=""col-12 grid-margin"">
            <div class=""profile-header"">
                <div class=""cover"">
                    <div class=""col-sm-10"" style=""float:left"">
                        <h4 class=""mb-3 mb-md-0"">My Profile</h4>
                    </div>
                    <div class=""col-sm-2"" style=""float:right"">
                        <button class=""btn btn-primary btn-icon-text btn-edit-profile"" id=""btnEditProfile"" style=""background-color: #058383 !important "">
                            <i data-feather=""edit"" class=""btn-icon-prepend""></i> Edit profile
                        </button>
                    </div>
                    
                </div>
            </div>
        </div>
    </div>
    <div class=""row profile-body"">
        <!-- left wrapper start -->
        <div class=""d-none d-md-block col-md-12 col-xl-12 left-wrapper"">
            <div class=""card rounded"">
                <div class=""card-body"">
           ");
            WriteLiteral("         <div class=\"d-flex align-items-center justify-content-between mb-2\">\n                        <h6 class=\"card-title mb-0\"></h6>\n                        ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("img", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagOnly, "bded577a68d591b4b6eefed452154f24364873466142", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_2);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_3);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_4);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\n                    </div>\n");
#nullable restore
#line 30 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Home\GetProfile.cshtml"
                     if (Model.User.PassportPath == null || Model.User.PassportPath == "")
                    {

#line default
#line hidden
#nullable disable
            WriteLiteral(@"                        <div>
                            <div class=""col-sm-10"" style=""float:left"">

                            </div>
                            <div class=""col-sm-2"" style=""float:right"">
                                <input type=""file"" />
                            </div>
                        </div>
");
#nullable restore
#line 40 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Home\GetProfile.cshtml"
                     }

#line default
#line hidden
#nullable disable
            WriteLiteral(@"                    <div class=""mt-3"">
                        <div class=""col-sm-2"" style=""float:left"">
                            <label class=""tx-11 font-weight-bold mb-0 text-uppercase"">First Name:</label>
                        </div>
                        <div class=""col-sm-10"">
                            <p class=""text-muted"" id=""editFirstname"">");
#nullable restore
#line 46 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Home\GetProfile.cshtml"
                                                                Write(Model.User.FirstName);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"</p>
                        </div>
                    </div>
                    <div class=""mt-3"">
                        <div class=""col-sm-2"" style=""float:left"">
                            <label class=""tx-11 font-weight-bold mb-0 text-uppercase"">Last Name:</label>
                        </div>
                        <div class=""col-sm-10"">
                            <p class=""text-muted"" id=""editLastname"">");
#nullable restore
#line 54 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Home\GetProfile.cshtml"
                                                               Write(Model.User.LastName);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"</p>
                        </div>
                    </div>
                    <div class=""mt-3"">
                        <div class=""col-sm-2"" style=""float:left"">
                            <label class=""tx-11 font-weight-bold mb-0 text-uppercase"">Email:</label>
                        </div>
                        <div class=""col-sm-10"">
                            <p class=""text-muted"" id=""editEmail"">");
#nullable restore
#line 62 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Home\GetProfile.cshtml"
                                                            Write(Model.User.Email);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"</p>
                        </div>
                    </div>
                    <div class=""mt-3"">
                        <div class=""col-sm-2"" style=""float:left"">
                            <label class=""tx-11 font-weight-bold mb-0 text-uppercase"">Phone Number:</label>
                        </div>
                        <div class=""col-sm-10"">
                            <p class=""text-muted"" id=""editPhone"">");
#nullable restore
#line 70 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Home\GetProfile.cshtml"
                                                            Write(Model.User.PhoneNumber);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"</p>
                        </div>
                    </div>
                    <div class=""mt-3"">
                        <div class=""col-sm-2"" style=""float:left"">
                            <label class=""tx-11 font-weight-bold mb-0 text-uppercase"">Alternate Phone Number:</label>
                        </div>
                        <div class=""col-sm-10"">
                            <p class=""text-muted"" id=""editAltPhone"">");
#nullable restore
#line 78 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Home\GetProfile.cshtml"
                                                               Write(Model.User.AlternatePhone);

#line default
#line hidden
#nullable disable
            WriteLiteral("</p>\n                        </div>\n                    </div>\n                </div>\n            </div>\n        </div>\n        <!-- left wrapper end -->\n        \n    </div>\n</div>");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<CPEA.Utilities.ViewModel.UserProfileVM> Html { get; private set; } = default!;
        #nullable disable
    }
}
#pragma warning restore 1591
