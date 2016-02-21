using BundleTransformer.Core.Builders;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Resolvers;
using BundleTransformer.Core.Transformers;
using System.Web.Optimization;

namespace SetGenerator.WebUI
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            var nullBuilder = new NullBuilder();
            var styleTransformer = new StyleTransformer();
            var scriptTransformer = new ScriptTransformer();
            var nullOrderer = new NullOrderer();

            // Replace a default bundle resolver in order to the debugging HTTP-handler
            // can use transformations of the corresponding bundle
            BundleResolver.Current = new CustomBundleResolver();

            var commonStylesBundle = new Bundle("~/bundles/CommonStyles");
            commonStylesBundle.Include(
                    "~/Content/tablednd.css",
                    "~/Content/bootstrap.css",
                    "~/Content/bootstrap-theme.css",
                    "~/Content/bootstrap-datepicker.css",
                    "~/Content/Site.less");
            commonStylesBundle.Builder = nullBuilder;
            commonStylesBundle.Orderer = nullOrderer;
            commonStylesBundle.Transforms.Add(styleTransformer);
            bundles.Add(commonStylesBundle);

            var commonScriptsBundle = new Bundle("~/bundles/CommonScripts");
            commonScriptsBundle.Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/jquery.tablednd.js",
                "~/Scripts/moment.js");
            commonScriptsBundle.Builder = nullBuilder;
            commonScriptsBundle.Transforms.Add(scriptTransformer);
            bundles.Add(commonScriptsBundle);

            var customScriptsBundle = new Bundle("~/bundles/CustomScripts");
            customScriptsBundle.Include(
                    "~/Scripts/Site/Site.js",
                    "~/Scripts/Site/Session.js",
                    "~/Scripts/Site/Namespace.js",
                    "~/Scripts/Dialog/Dialog.js",
                    "~/Scripts/Shared/_LogonPartial.js",
                    "~/Scripts/Home/Index.js",
                    "~/Scripts/Songs/Index.js",
                    "~/Scripts/Setlists/Index.js",
                    "~/Scripts/Setlists/Sets.js",
                    "~/Scripts/Gigs/Index.js",
                    "~/Scripts/Bands/Index.js",
                    "~/Scripts/Bands/Members.js");
            customScriptsBundle.Builder = nullBuilder;
            customScriptsBundle.Transforms.Add(scriptTransformer);
            bundles.Add(customScriptsBundle);

            var modernizrScriptsBundle = new Bundle("~/bundles/ModernizrScripts");
            modernizrScriptsBundle.Include(
                    "~/Scripts/modernizr-*");
            modernizrScriptsBundle.Builder = nullBuilder;
            modernizrScriptsBundle.Transforms.Add(scriptTransformer);
            bundles.Add(modernizrScriptsBundle);

            var bootstrapScriptsBundle = new Bundle("~/bundles/BootstrapScripts");
            bootstrapScriptsBundle.Include(
                     "~/Scripts/bootstrap.js",
                     "~/Scripts/bootstrap-datepicker.js",
                     "~/Scripts/respond.js");
            bootstrapScriptsBundle.Builder = nullBuilder;
            bootstrapScriptsBundle.Transforms.Add(scriptTransformer);
            bundles.Add(bootstrapScriptsBundle);
        }
    }
}
