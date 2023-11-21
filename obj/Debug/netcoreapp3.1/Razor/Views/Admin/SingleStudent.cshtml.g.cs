#pragma checksum "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "05c13e151f07cb344eee99fb62b6622283385331"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Admin_SingleStudent), @"mvc.1.0.view", @"/Views/Admin/SingleStudent.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"05c13e151f07cb344eee99fb62b6622283385331", @"/Views/Admin/SingleStudent.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"368f9781feefc3edff7f3832f6ea5b7d8b8e9b03", @"/Views/_ViewImports.cshtml")]
    #nullable restore
    public class Views_Admin_SingleStudent : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<CPEA.Utilities.DTO.StudentDetails>
    #nullable disable
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 1 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
  
    Layout = "~/Views/Shared/_adminLayout.cshtml";

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
            WriteLiteral("\r\n    <nav class=\"page-breadcrumb\">\r\n        <ol class=\"breadcrumb\">\r\n            <li class=\"breadcrumb-item\"><a");
            BeginWriteAttribute("href", " href=\"", 215, "\"", 263, 1);
#nullable restore
#line 9 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
WriteAttributeValue("", 222, Url.Action("RegisteredStudents","Admin"), 222, 41, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">Students</a></li>\r\n            <li class=\"breadcrumb-item active\" aria-current=\"page\">");
#nullable restore
#line 10 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                              Write(Model.StudentNumber);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"</li>
        </ol>
    </nav>

    <div class=""row"">
        <div class=""col-md-12"">
            <div class=""card"">
                <div class=""card-body"">
                    <div>
                        <h4 class=""mb-3 mb-md-0"">Student Details</h4>
                    </div>
                    <br />
                    <div class=""container-fluid d-flex justify-content-between"">
                        <div class=""col-lg-3 pl-0"">
                            <h5 class=""mb-0 mt-3 font-weight-normal mb-2""><span class=""text-muted"">Student Number :</span><b>");
#nullable restore
#line 24 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                                                        Write(Model.StudentNumber);

#line default
#line hidden
#nullable disable
            WriteLiteral("</b> </h5>\r\n                            <p>First Name : <b>");
#nullable restore
#line 25 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                          Write(Model.FirstName);

#line default
#line hidden
#nullable disable
            WriteLiteral("</b></p>\r\n                            <p>Last Name : <b>");
#nullable restore
#line 26 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                         Write(Model.LastName);

#line default
#line hidden
#nullable disable
            WriteLiteral("</b></p>\r\n                            <p>Reg. Date : <b>");
#nullable restore
#line 27 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                         Write(Model.RegisteredDate);

#line default
#line hidden
#nullable disable
            WriteLiteral("</b></p>\r\n                            <p>Student Status : <b>");
#nullable restore
#line 28 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                              Write(Model.Status);

#line default
#line hidden
#nullable disable
            WriteLiteral("</b></p>\r\n                        </div>\r\n                    </div>\r\n                    <hr />\r\n");
#nullable restore
#line 32 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                     if (Model != null && Model.StCourses != null && Model.StCourses.Count > 0)
                    {

#line default
#line hidden
#nullable disable
            WriteLiteral(@"                        <div class=""container-fluid mt-5 d-flex justify-content-center w-100"">
                            <div class=""table-responsive w-100"">
                                <div>
                                    <h4 class=""mb-3 mb-md-0"">Registered Courses</h4>
                                </div>
                                <br />
                                <table class=""table table-hover mb-0 "" id=""dataTableExample"">
                                    <thead>
                                        <tr>
                                            <th class=""pt-0"">Course Name</th>
                                            <th class=""pt-0"">Option Name</th>
                                            <th class=""pt-0"">Option Price</th>
                                            <th class=""pt-0"">Amount Paid</th>
                                            <th class=""pt-0"">Payment Status</th>
                                            <th class=""pt-0"">Registered ");
            WriteLiteral("Date</th>\r\n                                            <th class=\"pt-0\">Course Start Status</th>\r\n");
            WriteLiteral("                                            <th class=\"pt-0\">Certificate Issued</th>\r\n");
            WriteLiteral("\r\n                                        </tr>\r\n                                    </thead>\r\n                                    <tbody>\r\n\r\n");
#nullable restore
#line 58 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                         foreach (var item in Model.StCourses)
                                        {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                            <tr>\r\n                                                <td>");
#nullable restore
#line 61 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.CourseName));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>");
#nullable restore
#line 62 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.OptionName));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>₦ ");
#nullable restore
#line 63 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                 Write(Html.DisplayFor(modelItem => item.Price));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>₦ ");
#nullable restore
#line 64 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                 Write(Html.DisplayFor(modelItem => item.AmountPaid));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>\r\n");
#nullable restore
#line 66 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                     if (item.PaymentStatus == CPEA.Utilities.UserProgramPaymentStatusEnums.Paid)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span class=\"badge badge-success\">");
#nullable restore
#line 68 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                     Write(Html.DisplayFor(modelItem => item.PaymentStatus));

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>\r\n");
#nullable restore
#line 69 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }
                                                    else if (item.PaymentStatus == CPEA.Utilities.UserProgramPaymentStatusEnums.Deposited)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span class=\"badge badge-warning\">");
#nullable restore
#line 72 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                     Write(Html.DisplayFor(modelItem => item.PaymentStatus));

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>\r\n");
#nullable restore
#line 73 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                </td>\r\n                                                <td>");
#nullable restore
#line 75 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.RegisteredDate));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n\r\n                                                <td>\r\n");
#nullable restore
#line 78 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                     if (item.CourseStartStatus == CPEA.Utilities.UserCourseStatusEnums.Completed)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span class=\"badge badge-success\">");
#nullable restore
#line 80 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                     Write(Html.DisplayFor(modelItem => item.CourseStartStatus));

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>\r\n");
#nullable restore
#line 81 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }
                                                    else if (item.CourseStartStatus == CPEA.Utilities.UserCourseStatusEnums.Pending)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span class=\"badge badge-warning\">");
#nullable restore
#line 84 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                     Write(Html.DisplayFor(modelItem => item.CourseStartStatus));

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>\r\n");
#nullable restore
#line 85 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }
                                                    else if (item.CourseStartStatus == CPEA.Utilities.UserCourseStatusEnums.InProgress)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span class=\"badge badge-info-muted\">");
#nullable restore
#line 88 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                        Write(Html.DisplayFor(modelItem => item.CourseStartStatus));

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>\r\n");
#nullable restore
#line 89 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                </td>\r\n");
            WriteLiteral("                                                <td>\r\n");
#nullable restore
#line 98 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                     if (item.CertificateIssued == true)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span>True</span>\r\n");
#nullable restore
#line 101 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }
                                                    else
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span>False</span>\r\n");
#nullable restore
#line 105 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                </td>\r\n");
            WriteLiteral("\r\n                                            </tr>\r\n");
#nullable restore
#line 115 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                        }

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                                    </tbody>\r\n                                </table>\r\n                            </div>\r\n                        </div>\r\n");
#nullable restore
#line 121 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                    }

#line default
#line hidden
#nullable disable
            WriteLiteral("                    <hr />\r\n");
#nullable restore
#line 123 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                     if (Model != null && Model.StCerts != null && Model.StCerts.Count > 0)
                    {

#line default
#line hidden
#nullable disable
            WriteLiteral(@"                        <div class=""container-fluid mt-5 d-flex justify-content-center w-100"">
                            <div class=""table-responsive w-100"">
                                <div>
                                    <h4 class=""mb-3 mb-md-0"">Registered Certications</h4>
                                </div>
                                <br />
                                <table class=""table table-hover mb-0 myDataTable"">
                                    <thead>
                                        <tr>
");
            WriteLiteral(@"                                            <th class=""pt-0"">Name</th>
                                            <th class=""pt-0"">Exam Date</th>
                                            <th class=""pt-0"">Option Price</th>
                                            <th class=""pt-0"">Amount Paid</th>
                                            <th class=""pt-0"">Payment Status</th>
                                            <th class=""pt-0"">Registered Date</th>
                                            <th class=""pt-0"">Org. Name</th>
                                            <th class=""pt-0"">Mode</th>
                                            <th class=""pt-0"">Short Code</th>
                                            <th class=""pt-0"">Cert. Start Status</th>
");
            WriteLiteral("                                            <th class=\"pt-0\">Cert. Issued</th>\r\n");
            WriteLiteral("\r\n                                        </tr>\r\n                                    </thead>\r\n                                    <tbody>\r\n\r\n");
#nullable restore
#line 154 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                         foreach (var item in Model.StCerts)
                                        {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                            <tr>\r\n");
            WriteLiteral("                                                <td>");
#nullable restore
#line 159 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.Name));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>");
#nullable restore
#line 160 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.ExamDate));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>");
#nullable restore
#line 161 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.Price));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>");
#nullable restore
#line 162 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.AmountPaid));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>\r\n");
#nullable restore
#line 164 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                     if (item.PaymentStatus == CPEA.Utilities.UserProgramPaymentStatusEnums.Paid)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span class=\"badge badge-success\">");
#nullable restore
#line 166 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                     Write(Html.DisplayFor(modelItem => item.PaymentStatus));

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>\r\n");
#nullable restore
#line 167 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }
                                                    else if (item.PaymentStatus == CPEA.Utilities.UserProgramPaymentStatusEnums.Deposited)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span class=\"badge badge-warning\">");
#nullable restore
#line 170 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                     Write(Html.DisplayFor(modelItem => item.PaymentStatus));

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>\r\n");
#nullable restore
#line 171 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                </td>\r\n                                                <td>");
#nullable restore
#line 173 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.RegisteredDate));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>");
#nullable restore
#line 174 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.OrganisationName));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>");
#nullable restore
#line 175 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.Mode));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>");
#nullable restore
#line 176 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                               Write(Html.DisplayFor(modelItem => item.ShortCode));

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                                <td>\r\n");
#nullable restore
#line 178 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                     if (item.CourseStartStatus == CPEA.Utilities.UserCourseStatusEnums.Completed)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span class=\"badge badge-success\">");
#nullable restore
#line 180 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                     Write(Html.DisplayFor(modelItem => item.CourseStartStatus));

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>\r\n");
#nullable restore
#line 181 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }
                                                    else if (item.CourseStartStatus == CPEA.Utilities.UserCourseStatusEnums.Pending)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span class=\"badge badge-warning\">");
#nullable restore
#line 184 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                     Write(Html.DisplayFor(modelItem => item.CourseStartStatus));

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>\r\n");
#nullable restore
#line 185 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }
                                                    else if (item.CourseStartStatus == CPEA.Utilities.UserCourseStatusEnums.InProgress)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span class=\"badge badge-info-muted\">");
#nullable restore
#line 188 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                                                        Write(Html.DisplayFor(modelItem => item.CourseStartStatus));

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>\r\n");
#nullable restore
#line 189 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                </td>\r\n                                                <td>\r\n");
#nullable restore
#line 192 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                     if (item.CertificateIssued == true)
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span>True</span>\r\n");
#nullable restore
#line 195 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }
                                                    else
                                                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                        <span>False</span>\r\n");
#nullable restore
#line 199 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                                    }

#line default
#line hidden
#nullable disable
            WriteLiteral("                                                </td>\r\n\r\n                                            </tr>\r\n");
#nullable restore
#line 203 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                                        }

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                                    </tbody>\r\n                                </table>\r\n                            </div>\r\n                        </div>\r\n");
#nullable restore
#line 209 "C:\Users\CAROLINE\source\repos\CPEA\CPEA\Views\Admin\SingleStudent.cshtml"
                    }

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </div>\r\n\r\n\r\n\r\n\r\n");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<CPEA.Utilities.DTO.StudentDetails> Html { get; private set; } = default!;
        #nullable disable
    }
}
#pragma warning restore 1591
