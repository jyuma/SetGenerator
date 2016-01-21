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
                     "~/Content/bootstrap.css",
                     "~/Content/bootstrap-theme.css",
                     "~/Content/awesome-bootstrap-checkbox.css",
                     "~/Content/Site.less");
            commonStylesBundle.Builder = nullBuilder;
            commonStylesBundle.Orderer = nullOrderer;
            commonStylesBundle.Transforms.Add(styleTransformer);
            bundles.Add(commonStylesBundle);

            var commonScriptsBundle = new Bundle("~/bundles/CommonScripts");
            commonScriptsBundle.Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/knockout-{version}.js");
            commonScriptsBundle.Builder = nullBuilder;
            commonScriptsBundle.Transforms.Add(scriptTransformer);
            bundles.Add(commonScriptsBundle);

            var customScriptsBundle = new Bundle("~/bundles/CustomScripts");
            customScriptsBundle.Include(
                    "~/Scripts/Custom/Site/Site.js",
                    "~/Scripts/Custom/Site/Session.js",
                    "~/Scripts/Custom/Site/Namespace.js",
                    "~/Scripts/Custom/Dialog/Dialog.js",
                    "~/Scripts/Custom/Shared/_Layout.js",
                    "~/Scripts/Custom/Home/Index.js",
                    "~/Scripts/Custom/Songs/Index.js",
                    "~/Scripts/Custom/SetLists/Index.js",
                    "~/Scripts/Custom/SetLists/Sets.js",
                    "~/Scripts/Custom/Gigs/Index.js");
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
                     "~/Scripts/respond.js");
            bootstrapScriptsBundle.Builder = nullBuilder;
            bootstrapScriptsBundle.Transforms.Add(scriptTransformer);
            bundles.Add(bootstrapScriptsBundle);
        }
    }
}
