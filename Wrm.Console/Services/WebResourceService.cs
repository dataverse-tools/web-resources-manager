using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using NLog;
using Wrm.ConsoleApp.Extensions;
using Wrm.Model;
using Wrm.Model.Constants;
using Wrm.Model.Enums;
using Wrm.Repositories;
using Xrm.Plugins.Common.Extensions;


namespace Wrm.Services
{
    public sealed class WebResourceService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly WebResourceRepository _webResourceRepo;


        public WebResourceService(IOrganizationService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            _webResourceRepo = new WebResourceRepository(service);
        }

        public void Init(InitOptions options)
        {
            var resources = _webResourceRepo.GetAll(options.Prefixes.ToArray());

            _logger.Info($"Serializing...");

            var config = resources.Select(wr =>
            {
                var name = wr.GetAttributeValue<string>(WebResource.name);
                var path = name.NormalizePath(options.WebResourcesRoot, wr.GetOptionSetValueAsEnum<WebResourceType>(WebResource.webresourcetype));

                return new WebResourceConfig
                {
                    Name = name,
                    DisplayName = wr.GetAttributeValue<string>(WebResource.displayname) ?? name.GetDisplayName(),
                    Description = wr.GetAttributeValue<string>(WebResource.description) ?? string.Empty,
                    Path = path
                };
            });
            var serialized = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(options.ConfigFilePath, serialized);

            _logger.Info("Done processing Web Resources.");
        }

        public void Push(PushOptions options)
        {
            var config = ReadConfig(options);
            var publishList = new List<Guid>(config.Count);

            foreach (var wrCfg in config)
            {
                _logger.Info($"Processing Web Resource {wrCfg.Name}...");

                var existingResource = _webResourceRepo.Get(wrCfg.Name);

                if (!options.Overwrite && existingResource != null)
                {
                    _logger.Info($"Web Resource {wrCfg.Name} already exists and overwriting is disabled. Skipping.");
                    continue;
                }

                if (existingResource == null)
                {
                    var type = wrCfg.Path.GetWebResourceType();
                    if (type == null)
                    {
                        _logger.Warn($"Web Resource {wrCfg.Path} contains no extension or has the unsupported one. Skipping.");
                        continue;
                    }

                    existingResource = new Entity(WebResource.LogicalName);
                    existingResource[WebResource.name] = wrCfg.Name;
                    existingResource[WebResource.webresourcetype] = type.Value.ToOptionSetValue();
                }

                existingResource[WebResource.displayname] = string.IsNullOrEmpty(wrCfg.DisplayName) ? wrCfg.Name.GetDisplayName() : wrCfg.DisplayName;

                var file = File.ReadAllBytes(wrCfg.Path);
                var filecontent = Convert.ToBase64String(file);
                existingResource[WebResource.content] = filecontent;

                _webResourceRepo.CreateOrUpdate(existingResource, wrCfg.Name);

                publishList.Add(existingResource.Id);
            }

            _webResourceRepo.Publish(publishList);
        }

        public void Pull(PullOptions _)
        {
            throw new NotImplementedException();
        }

        public void Delete(DeleteOptions options)
        {
            var config = ReadConfig(options);

            var deletedCount = 0;

            foreach (var wrCfg in config)
            {
                _logger.Info($"Processing Web Resource {wrCfg.Name}...");

                var existingResource = _webResourceRepo.Get(wrCfg.Name);
                if (existingResource == null)
                {
                    _logger.Info($"Web Resource {wrCfg.Name} was not found. Skipping.");
                    continue;
                }

                if (_webResourceRepo.CanBeDeleted(existingResource.Id, wrCfg.Name))
                {
                    _webResourceRepo.Delete(existingResource.Id, wrCfg.Name);
                    deletedCount++;
                }
            }

            if (deletedCount == 0)
            {
                return;
            }

            _webResourceRepo.PublishAll();
        }


        private static List<WebResourceConfig> ReadConfig(Options options)
        {
            var configText = File.ReadAllText(options.ConfigFilePath);
            return JsonConvert.DeserializeObject<List<WebResourceConfig>>(configText);
        }
    }
}
