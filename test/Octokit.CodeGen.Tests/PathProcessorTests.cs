using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OneOf;
using Xunit;

namespace Octokit.CodeGen.Tests
{
    using ResponseProperty = OneOf<PrimitiveResponseProperty,
                                   ObjectResponseProperty,
                                   ListOfPrimitiveTypeProperty,
                                   ListOfObjectsProperty>;

    using RequestProperty = OneOf<PrimitiveRequestProperty,
                                  ArrayRequestProperty,
                                  ObjectRequestProperty,
                                  StringEnumRequestProperty>;


    public class PathProcessorTests
    {
        [Fact]
        public async Task Process_ForPathWithOneVerb_ExtractsInformation()
        {
            var path = TestFixtureLoader.LoadPathWithGet();

            var results = await PathProcessor.Process(path);
            var result = Assert.Single(results);

            Assert.Equal("/marketplace_listing/accounts/{account_id}", result.Path);
            Assert.Single(result.Verbs);

            var get = result.Verbs.First();
            Assert.Equal(HttpMethod.Get, get.Method);
            Assert.Equal("application/vnd.github.v3+json", get.AcceptHeader);
            Assert.Equal("Check if a GitHub account is associated with any Marketplace listing", get.Summary);
            Assert.Equal("Some description goes here", get.Description);
            Assert.Equal("https://developer.github.com/v3/apps/marketplace/#check-if-a-github-account-is-associated-with-any-marketplace-listing", get.ExternalDocumentation);

            var parameter = Assert.Single(get.Parameters);

            Assert.Equal("account_id", parameter.Name);
            Assert.Equal("path", parameter.In);
            Assert.Equal("integer", parameter.Type);
            Assert.True(parameter.Required);

            var response = Assert.Single(get.Responses);

            Assert.Equal("200", response.StatusCode);
            Assert.Equal("application/json", response.ContentType);
            var responseContent = response.Content.AsT0;
            Assert.Equal("object", responseContent.Type);

            var content = Assert.IsType<ObjectResponseContent>(responseContent);

            Assert.Single(content.Properties.PrimitiveProperties().Where(p => p.Name == "url" && p.Type == "string"));

            var nestedObject = Assert.Single(content.Properties.ObjectProperties().Where(p => p.Name == "marketplace_pending_change" && p.Type == "object"));

            Assert.Single(nestedObject.Properties.PrimitiveProperties().Where(p => p.Name == "unit_count" && p.Type == "string"));

            var nestedNestedObject = Assert.Single(nestedObject.Properties.ObjectProperties().Where(p => p.Name == "plan" && p.Type == "object"));

            Assert.Single(nestedNestedObject.Properties.PrimitiveProperties().Where(p => p.Name == "yearly_price_in_cents" && p.Type == "number"));
        }

        [Fact]
        public async Task Process_ForPathWithTwoVerbs_ExtractsInformationForGet()
        {
            var stream = TestFixtureLoader.LoadPathWithGetAndPost();

            var results = await PathProcessor.Process(stream);
            var result = Assert.Single(results);

            Assert.Equal("/repos/{owner}/{repo}/commits/{commit_sha}/comments", result.Path);
            Assert.Equal(2, result.Verbs.Count);

            var get = result.Verbs.First();
            Assert.Equal(HttpMethod.Get, get.Method);
            Assert.Equal("application/vnd.github.v3+json", get.AcceptHeader);
            Assert.Equal("List comments for a single commit", get.Summary);
            Assert.Equal("Use the `:commit_sha` to specify the commit that will have its comments listed.", get.Description);

            // required parameters
            Assert.Single(get.Parameters.Where(p => p.Name == "owner" && p.In == "path" && p.Type == "string" && p.Required));
            Assert.Single(get.Parameters.Where(p => p.Name == "repo" && p.In == "path" && p.Type == "string" && p.Required));
            Assert.Single(get.Parameters.Where(p => p.Name == "commit_sha" && p.In == "path" && p.Type == "string" && p.Required));

            // optional parameters
            Assert.Single(get.Parameters.Where(p => p.Name == "per_page" && p.In == "query" && p.Type == "integer" && !p.Required));
            Assert.Single(get.Parameters.Where(p => p.Name == "page" && p.In == "query" && p.Type == "integer" && !p.Required));

            // no request parameters needed
            Assert.Null(get.RequestBody);

            var response = Assert.Single(get.Responses);

            Assert.Equal("200", response.StatusCode);
            Assert.Equal("application/json", response.ContentType);
            var content = response.Content.AsT1;
            Assert.Equal("array", content.Type);

            var responseContent = Assert.IsType<ArrayResponseContent>(content);

            Assert.Single(responseContent.ItemProperties.PrimitiveProperties().Where(p => p.Name == "html_url" && p.Type == "string"));

            var objectPropType = responseContent.ItemProperties.ObjectProperties().Single(p => p.Name == "user" && p.Type == "object");
            var nestedObject = Assert.IsType<ObjectResponseProperty>(objectPropType);

            Assert.Single(nestedObject.Properties.PrimitiveProperties().Where(p => p.Name == "login" && p.Type == "string"));
        }

        [Fact]
        public async Task Process_ForPathWithTwoVerbs_ExtractsInformationForPost()
        {
            var stream = TestFixtureLoader.LoadPathWithGetAndPost();

            var results = await PathProcessor.Process(stream);
            var result = Assert.Single(results);

            Assert.Equal("/repos/{owner}/{repo}/commits/{commit_sha}/comments", result.Path);
            Assert.Equal(2, result.Verbs.Count);

            var post = result.Verbs.Last();
            Assert.Equal(HttpMethod.Post, post.Method);
            Assert.Equal("application/vnd.github.v3+json", post.AcceptHeader);
            Assert.Equal("Create a commit comment", post.Summary);
            Assert.Equal("Create a comment for a commit using its `:commit_sha`.\n\nThis endpoint triggers [notifications](https://help.github.com/articles/about-notifications/). Creating content too quickly using this endpoint may result in abuse rate limiting. See \"[Abuse rate limits](https://developer.github.com/v3/#abuse-rate-limits)\" and \"[Dealing with abuse rate limits](https://developer.github.com/v3/guides/best-practices-for-integrators/#dealing-with-abuse-rate-limits)\" for details.", post.Description);

            // required parameters
            Assert.Single(post.Parameters.Where(p => p.Name == "owner" && p.In == "path" && p.Type == "string" && p.Required));
            Assert.Single(post.Parameters.Where(p => p.Name == "repo" && p.In == "path" && p.Type == "string" && p.Required));
            Assert.Single(post.Parameters.Where(p => p.Name == "commit_sha" && p.In == "path" && p.Type == "string" && p.Required));

            Assert.NotNull(post.RequestBody);

            Assert.Equal("application/json", post.RequestBody.ContentType);
            var requestContent = post.RequestBody.Content.AsT0;
            Assert.Equal("object", requestContent.Type);

            Assert.Single(requestContent.Properties.PrimitiveProperties().Where(p => p.Name == "body" && p.Type == "string" && p.Required));
            Assert.Single(requestContent.Properties.PrimitiveProperties().Where(p => p.Name == "path" && p.Type == "string" && !p.Required));
            Assert.Single(requestContent.Properties.PrimitiveProperties().Where(p => p.Name == "position" && p.Type == "integer" && !p.Required));

            // TODO: this parameter is deprecated in the schema - we should not make it available to callers
            Assert.Single(requestContent.Properties.PrimitiveProperties().Where(p => p.Name == "line" && p.Type == "integer"));

            // TODO: we need to surface which parameters are required here, and
            //       repurposing the response parser means we don't have that
            //       type immediately available

            Assert.Single(post.Responses);
            var response = post.Responses.First();

            Assert.Equal("201", response.StatusCode);
            Assert.Equal("application/json", response.ContentType);
            var content = response.Content.AsT0;
            Assert.Equal("object", content.Type);

            var responseContent = Assert.IsType<ObjectResponseContent>(content);

            Assert.Single(responseContent.Properties.PrimitiveProperties().Where(p => p.Name == "html_url" && p.Type == "string"));

            var nestedObject = Assert.Single(responseContent.Properties.ObjectProperties().Where(p => p.Name == "user" && p.Type == "object"));

            Assert.Single(nestedObject.Properties.PrimitiveProperties().Where(p => p.Name == "login" && p.Type == "string"));
        }

        [Fact]
        public async Task Process_ForPathWithThreeVerb_ExtractsInformationForGet()
        {
            var stream = TestFixtureLoader.LoadPathWithGetPutAndDelete();

            var results = await PathProcessor.Process(stream);
            var result = Assert.Single(results);

            Assert.Equal("/user/following/{username}", result.Path);
            Assert.Equal(3, result.Verbs.Count);

            var get = result.Verbs.First(v => v.Method == HttpMethod.Get);

            Assert.Single(get.Parameters);
            Assert.Single(get.Parameters.Where(p => p.Name == "username" && p.Type == "string" && p.In == "path" && p.Required));

            Assert.Equal(2, get.Responses.Count);
        }

        [Fact]
        public async Task Process_ForPathWithThreeVerb_ExtractsInformationForPut()
        {
            var stream = TestFixtureLoader.LoadPathWithGetPutAndDelete();

            var results = await PathProcessor.Process(stream);
            var result = Assert.Single(results);

            var put = result.Verbs.First(v => v.Method == HttpMethod.Put);

            Assert.Equal("application/vnd.github.v3+json", put.AcceptHeader);

            Assert.Single(put.Parameters);
            Assert.Single(put.Parameters.Where(p => p.Name == "username" && p.Type == "string" && p.In == "path" && p.Required));

            Assert.Single(put.Responses);
        }

        [Fact]
        public async Task Process_ForPathWithThreeVerb_ExtractsInformationForDelete()
        {
            var stream = TestFixtureLoader.LoadPathWithGetPutAndDelete();

            var results = await PathProcessor.Process(stream);
            var result = Assert.Single(results);

            var delete = result.Verbs.First(v => v.Method == HttpMethod.Delete);

            Assert.Single(delete.Parameters);
            Assert.Single(delete.Parameters.Where(p => p.Name == "username" && p.Type == "string" && p.In == "path" && p.Required));

            Assert.Single(delete.Responses);
        }

        [Fact]
        public async Task Process_ForUserReposPath_IncludesEnumValues()
        {
            var stream = TestFixtureLoader.LoadUserReposEndpoint();

            var results = await PathProcessor.Process(stream);
            var result = Assert.Single(results);

            var get = Assert.Single(result.Verbs.Where(v => v.Method == HttpMethod.Get));

            var visibility = Assert.Single(get.Parameters.Where(p => p.Name == "visibility" && p.Type == "string" && p.In == "query"));

            Assert.Contains("all", visibility.Values);
            Assert.Contains("public", visibility.Values);
            Assert.Contains("private", visibility.Values);

            Assert.Equal("all", visibility.Default);

            var type = Assert.Single(get.Parameters.Where(p => p.Name == "type" && p.Type == "string" && p.In == "query"));

            Assert.Contains("all", type.Values);
            Assert.Contains("owner", type.Values);
            Assert.Contains("public", type.Values);
            Assert.Contains("private", type.Values);
            Assert.Contains("member", type.Values);

            Assert.Equal("all", type.Default);

            var sort = Assert.Single(get.Parameters.Where(p => p.Name == "sort" && p.Type == "string" && p.In == "query"));

            Assert.Contains("created", sort.Values);
            Assert.Contains("updated", sort.Values);
            Assert.Contains("pushed", sort.Values);
            Assert.Contains("full_name", sort.Values);

            Assert.Equal("full_name", sort.Default);

            var direction = Assert.Single(get.Parameters.Where(p => p.Name == "direction" && p.Type == "string" && p.In == "query"));

            Assert.Contains("asc", direction.Values);
            Assert.Contains("desc", direction.Values);

            Assert.Null(direction.Default);

            Assert.Single(get.Parameters.Where(p => p.Name == "per_page" && p.Type == "integer" && p.In == "query"));

            Assert.Single(get.Parameters.Where(p => p.Name == "page" && p.Type == "integer" && p.In == "query"));
        }

        [Fact]
        public async Task Process_ForUserReposPath_IncludesOptionalParametersOnPost()
        {
            var stream = TestFixtureLoader.LoadUserReposEndpoint();

            var results = await PathProcessor.Process(stream);
            var result = Assert.Single(results);

            var post = Assert.Single(result.Verbs.Where(v => v.Method == HttpMethod.Post));

            var requestBody = post.RequestBody.Content.AsT0;
            Assert.Single(requestBody.Properties.PrimitiveProperties().Where(p => p.Name == "name" && p.Type == "string" && p.Required));

            var visibility = Assert.Single(requestBody.Properties.StringEnumRequestProperties().Where(p => p.Name == "visibility" && p.Type == "string" && !p.Required));

            Assert.Contains("public", visibility.Values);
            Assert.Contains("private", visibility.Values);
            Assert.Contains("internal", visibility.Values);

            Assert.Single(post.Responses);
        }

        [Fact]
        public async Task Process_ForTopicsPath_ReturnsStringArrayOnRequest()
        {
            var stream = TestFixtureLoader.LoadTopicsRoute();

            var results = await PathProcessor.Process(stream);
            var result = Assert.Single(results);

            var get = Assert.Single(result.Verbs.Where(v => v.Method == HttpMethod.Put));

            var objectContent = get.RequestBody.Content.AsT0;
            var property = Assert.Single(objectContent.Properties.ArrayRequestProperties());

            Assert.Equal("names", property.Name);
            Assert.Equal("array", property.Type);
            var array = Assert.IsType<ArrayRequestProperty>(property);
            Assert.Equal("string", array.ArrayType);
        }

        [Fact]
        public async Task Process_ForTopicsPath_ReturnsStringArrayOnResponse()
        {
            var stream = TestFixtureLoader.LoadTopicsRoute();

            var results = await PathProcessor.Process(stream);
            var result = Assert.Single(results);

            var get = Assert.Single(result.Verbs.Where(v => v.Method == HttpMethod.Put));

            var response = Assert.Single(get.Responses);
            var objectContent = response.Content.AsT0;

            var property = Assert.Single(objectContent.Properties);

            var listProperty = property.AsT2;

            Assert.Equal("names", listProperty.Name);
            Assert.Equal("array", listProperty.Type);
            Assert.Equal("string", listProperty.ItemType);
        }
    }

    public static class PropertyExtensions
    {
        public static IEnumerable<PrimitiveResponseProperty> PrimitiveProperties(this List<ResponseProperty> properties)
        {
            return properties.Where(p => p.IsT0).Select(p => p.AsT0);
        }

        public static IEnumerable<ObjectResponseProperty> ObjectProperties(this List<ResponseProperty> properties)
        {
            return properties.Where(p => p.IsT1).Select(p => p.AsT1);
        }

        public static IEnumerable<PrimitiveRequestProperty> PrimitiveProperties(this List<RequestProperty> properties)
        {
            return properties.Where(p => p.IsT0).Select(p => p.AsT0);
        }

        public static IEnumerable<ArrayRequestProperty> ArrayRequestProperties(this List<RequestProperty> properties)
        {
            return properties.Where(p => p.IsT1).Select(p => p.AsT1);
        }

        public static IEnumerable<ObjectRequestProperty> ObjectRequestProperties(this List<RequestProperty> properties)
        {
            return properties.Where(p => p.IsT2).Select(p => p.AsT2);
        }


        public static IEnumerable<StringEnumRequestProperty> StringEnumRequestProperties(this List<RequestProperty> properties)
        {
            return properties.Where(p => p.IsT3).Select(p => p.AsT3);
        }
    }

}
