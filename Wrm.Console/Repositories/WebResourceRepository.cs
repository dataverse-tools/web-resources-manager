using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NLog;
using Wrm.Model.Constants;


namespace Wrm.Repositories
{
    public sealed class WebResourceRepository
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IOrganizationService _service;


        public WebResourceRepository(IOrganizationService service)
        {
            _service = service;
        }


        public List<Entity> GetAll(params string[] prefixes)
        {
            _logger.Info("Fetching Web Resources...");

            var cols = new ColumnSet(WebResource.name, WebResource.displayname, WebResource.description, WebResource.webresourcetype);

            var query = new QueryExpression(WebResource.LogicalName)
            {
                ColumnSet = cols,
                Criteria =
                {
                    FilterOperator = LogicalOperator.Or
                },
                Orders =
                {
                    new OrderExpression(WebResource.name, OrderType.Ascending)
                },
                PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    ReturnTotalRecordCount = true
                }
            };

            foreach (var prefix in prefixes)
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    continue;
                }

                var searchPrefix = prefix.EndsWith("_") ? prefix : prefix + "_";

                query.Criteria.AddCondition(WebResource.name, ConditionOperator.BeginsWith, searchPrefix);
            }

            var result = new List<Entity>();

            do
            {
                _logger.Info($"Processing page {query.PageInfo.PageNumber}...");

                var entities = _service.RetrieveMultiple(query);

                _logger.Info($"{entities.TotalRecordCount} records retrieved.");

                result.AddRange(entities.Entities);

                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = entities.PagingCookie;

                if (entities.MoreRecords)
                {
                    _logger.Info($"More records found, repeating fetch...");
                    continue;
                }

                break;
            }
            while (true);

            _logger.Info($"Total {result.Count} records found.");

            return result;
        }

        public Entity Get(string name, ColumnSet columnSet = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _logger.Debug($"Fetching metadata for Web Resource {name}...");

            var query = new QueryExpression(WebResource.LogicalName)
            {
                TopCount = 2,
                ColumnSet = columnSet ?? new ColumnSet(false),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(WebResource.name, ConditionOperator.Equal, name)
                    }
                }
            };

            return _service.RetrieveMultiple(query).Entities.SingleOrDefault();
        }

        public void CreateOrUpdate(Entity webResource, string name)
        {
            if (webResource == null)
            {
                throw new ArgumentNullException(nameof(webResource));
            }

            if (webResource.Id == Guid.Empty)
            {
                webResource.Id = _service.Create(webResource);
                _logger.Info($"Added Web Resource {name}.");
            }
            else
            {
                _service.Update(webResource);
                _logger.Info($"Updated Web Resource {name}.");
            }
        }

        public bool CanBeDeleted(Guid webResourceId, string name)
        {
            if (webResourceId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(webResourceId));
            }

            var retrieveDependenciesForDeleteRequest = new RetrieveDependenciesForDeleteRequest
            {
                ComponentType = 61, // Web Resource
                ObjectId = webResourceId
            };
            var retrieveDependenciesForDeleteResponse = (RetrieveDependenciesForDeleteResponse)_service.Execute(retrieveDependenciesForDeleteRequest);
            if (retrieveDependenciesForDeleteResponse.EntityCollection.Entities.Count > 0)
            {
                _logger.Warn($"Web Resource {name} has {retrieveDependenciesForDeleteResponse.EntityCollection.Entities.Count} dependencies and cannot be deleted. Skipping.");
                return false;
            }

            return true;
        }

        public void Delete(Guid webResourceId, string name)
        {
            if (webResourceId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(webResourceId));
            }

            _service.Delete(WebResource.LogicalName, webResourceId);
            _logger.Info($"Deleted Web Resource {name}.");
        }

        public void Publish(List<Guid> webResourceIdList)
        {
            if (webResourceIdList.Count == 0)
            {
                _logger.Info($"Nothing to publish. Exiting.");
                return;
            }

            _logger.Info($"Publishing {webResourceIdList.Count} web resource(s)...");

            var publishXmlEntries = webResourceIdList.Select(id => $"<webresource>{id}</webresource>");
            var publishXml = string.Join("", publishXmlEntries);
            var publishRequest = new PublishXmlRequest
            {
                ParameterXml = $"<importexportxml><webresources>{publishXml}</webresources></importexportxml>"
            };
            _service.Execute(publishRequest);

            _logger.Info($"Published.");
        }

        public void PublishAll()
        {
            _logger.Info("Publishing all...");

            _service.Execute(new PublishAllXmlRequest());

            _logger.Info("Published.");
        }
    }
}
