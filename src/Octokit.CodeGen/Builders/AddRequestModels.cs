using System;
using System.Collections.Generic;
using System.Linq;

namespace Octokit.CodeGen
{
    using TypeBuilderFunc = System.Func<PathMetadata, ApiClientFileMetadata, ApiClientFileMetadata>;

    public partial class Builders
    {
        public static readonly TypeBuilderFunc AddRequestModels = (metadata, data) =>
        {
            foreach (var verb in metadata.Verbs)
            {
                if (verb.RequestBody == null)
                {
                    continue;
                }

                var classNamePrefix = GetClassName(metadata);
                var model = new ApiResponseModelMetadata
                {
                    Kind = "request",
                    Name = $"{classNamePrefix}Request",
                    Method = verb.Method,
                };

                verb.RequestBody.Content.Switch(
                  objectRequest =>
                  {
                      foreach (var property in objectRequest.Properties)
                      {
                          property.Switch(primitive =>
                          {
                              model.Properties.Add(new ApiResponseModelProperty
                              {
                                  Name = GetPropertyName(primitive.Name),
                                  Type = primitive.Type,
                                  // TODO: what about required?
                              });
                          },
                          array =>
                          {
                              model.Properties.Add(new ApiResponseModelProperty
                              {
                                  Name = GetPropertyName(array.Name),
                                  Type = "List<string>",
                                  // TODO: what about required?
                              });
                          },
                          obj =>
                          {
                              throw new NotImplementedException();
                          },
                          stringEnum =>
                          {
                              throw new NotImplementedException();
                          });
                      }
                  },
                  stringRequest =>
                  {

                  },
                  stringArray =>
                  {

                  });

                data.RequestModels.Add(model);
            }

            data.RequestModels = data.RequestModels.Distinct(ApiModelCompararer.Default).ToList();

            return data;
        };
    }
}
