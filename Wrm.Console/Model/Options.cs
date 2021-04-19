using System.Collections.Generic;
using CommandLine;


namespace Wrm.Model
{
    public abstract class Options
    {
        [Option('c', "connection", HelpText = "Dataverse connection string", Required = true)]
        public string ConnectionString { get; set; }

        [Option('p', "config", HelpText = "Path to configuration file", Required = true)]
        public string ConfigFilePath { get; set; }

        [Option('o', "overwrite", HelpText = "Controls if launched action will overwrite existing files (config or web resources)", Default = false)]
        public bool Overwrite { get; set; }

        [Option('q', "quiet", HelpText = "Do not ask for user input", Required = false, Default = false)]
        public bool Quiet { get; set; }
    }

    [Verb("init", HelpText = "Generates config file for all available deployed Web Resources")]
    public sealed class InitOptions : Options
    {
        [Option('x', "prefix", HelpText = "Prefixes of Web Resources to fetch", Required = true, Separator = ',')]
        public IEnumerable<string> Prefixes { get; set; }

        [Option('r', "root", HelpText = "Path to the root Web Resources folder", Required = true)]
        public string WebResourcesRoot { get; set; }
    }

    [Verb("push", HelpText = "Pushes local files to Dataverse")]
    public sealed class PushOptions : Options { }

    [Verb("pull", HelpText = "Pulls web resources from Dataverse")]
    public sealed class PullOptions : Options { }

    [Verb("delete", HelpText = "Deletes web resources from Dataverse")]
    public sealed class DeleteOptions : Options { }
}
