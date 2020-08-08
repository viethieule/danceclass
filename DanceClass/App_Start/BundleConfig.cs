using System.Web;
using System.Web.Optimization;

namespace DanceClass
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/misc").Include(
                      "~/Scripts/jquery.slimscroll.js",
                      "~/Scripts/fastclick.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/adminlte").Include(
                      "~/Scripts/adminlte.js"));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                      "~/Scripts/moment.js",
                      "~/Scripts/moment-with-locales.min.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/iCheck").Include(
                      "~/Scripts/icheck.min.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/pace").Include(
                      "~/Scripts/pace.min.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/bootstrap-timepicker").Include(
                      "~/Scripts/bootstrap-timepicker.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-datepicker").Include(
                      "~/Scripts/bootstrap-datepicker.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/select2").Include(
                      "~/Scripts/select2.full.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/dataTables").Include(
                      "~/Scripts/jquery.dataTables.min.js",
                      "~/Scripts/dataTables.bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/inputMask").Include(
                      "~/Scripts/jquery.inputmask.js",
                      "~/Scripts/jquery.inputmask.date.extensions.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.css",
                      "~/Content/ionicons.css",
                      "~/Content/AdminLTE.css",
                      "~/Content/skin-black.css",
                      "~/Content/pace.min.css"
            ));
            
            bundles.Add(new StyleBundle("~/Content/bootstrap-datepicker").Include(
                      "~/Content/bootstrap-datepicker.min.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrap-timepicker").Include(
                      "~/Content/bootstrap-timepicker.min.css"));

            bundles.Add(new StyleBundle("~/Content/select2").Include(
                      "~/Content/select2.min.css"));
            
            bundles.Add(new StyleBundle("~/Content/dataTables").Include(
                      "~/Content/dataTables.bootstrap.min.css"));

            bundles.Add(new StyleBundle("~/Content/adminlte").Include(
                      "~/Content/AdminLTE.css",
                      "~/Content/skin-black.css"
            ));
        }
    }
}
