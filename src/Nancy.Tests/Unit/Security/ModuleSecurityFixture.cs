
namespace Nancy.Tests.Unit.Security
{
    using System;
    using System.Collections.Generic;

    using FakeItEasy;
    using Nancy.Responses;
    using Nancy.Security;
    using Nancy.Tests.Fakes;

    using Xunit;

    public class ModuleSecurityFixture
    {
        [Fact]
        public void Should_add_an_item_to_the_start_of_the_begin_pipeline_when_RequiresAuthentication_enabled()
        {
            var module = new FakeHookedModule(A.Fake<BeforePipeline>());

            module.RequiresAuthentication();

            A.CallTo(() => module.Before.AddItemToEndOfPipeline(A<Func<NancyContext, Response>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Should_add_two_items_to_the_start_of_the_begin_pipeline_when_RequiresClaims_enabled()
        {
            var module = new FakeHookedModule(A.Fake<BeforePipeline>());

            module.RequiresClaims(new[] { string.Empty });

            A.CallTo(() => module.Before.AddItemToEndOfPipeline(A<Func<NancyContext, Response>>.Ignored)).MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Fact]
        public void Should_add_two_items_to_the_start_of_the_begin_pipeline_when_RequiresValidatedClaims_enabled()
        {
            var module = new FakeHookedModule(A.Fake<BeforePipeline>());

            module.RequiresValidatedClaims(c => false);

            A.CallTo(() => module.Before.AddItemToStartOfPipeline(A<Func<NancyContext, Response>>.Ignored)).MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Fact]
        public void Should_return_unauthorized_response_with_RequiresAuthentication_enabled_and_no_username()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresAuthentication();

            var result = module.Before.Invoke(new NancyContext());

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void Should_return_unauthorized_response_with_RequiresAuthentication_enabled_and_blank_username()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresAuthentication();

            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser(String.Empty)
                              };
            
            var result = module.Before.Invoke(context);

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void Should_return_null_with_RequiresAuthentication_enabled_and_username_provided()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresAuthentication();

            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser("Bob")
                              };

            var result = module.Before.Invoke(context);

            result.ShouldBeNull();
        }

        [Fact]
        public void Should_return_unauthorized_response_with_RequiresClaims_enabled_and_no_username()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresClaims(new[] { string.Empty });

            var result = module.Before.Invoke(new NancyContext());

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void Should_return_unauthorized_response_with_RequiresClaims_enabled_and_blank_username()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresClaims(new[] { string.Empty });
            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser(String.Empty)
                              };

            var result = module.Before.Invoke(context);

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void Should_return_unauthorized_response_with_RequiresValidatedClaims_enabled_and_no_username()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresValidatedClaims(c => false);

            var result = module.Before.Invoke(new NancyContext());

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void Should_return_unauthorized_response_with_RequiresValidatedClaims_enabled_and_blank_username()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresValidatedClaims(c => false);
            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser(String.Empty)
                              };

            var result = module.Before.Invoke(context);

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void Should_return_forbidden_response_with_RequiresClaims_enabled_but_nonmatching_claims()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresClaims(new[] { "Claim1" });
            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser("username", new string[] {"Claim2", "Claim3"})
                              };

            var result = module.Before.Invoke(context);

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Should_return_forbidden_response_with_RequiresClaims_enabled_but_claims_key_missing()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresClaims(new[] { "Claim1" });
            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser("username")
                              };

            var result = module.Before.Invoke(context);

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Should_return_forbidden_response_with_RequiresClaims_enabled_but_not_all_claims_met()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresClaims(new[] { "Claim1", "Claim2" });
            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser("username", new[] {"Claim2"})
                              };

            var result = module.Before.Invoke(context);

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Should_return_null_with_RequiresClaims_and_all_claims_met()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresClaims(new[] { "Claim1", "Claim2" });
            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser("username", new[] {"Claim1", "Claim2", "Claim3"})
                              };

            var result = module.Before.Invoke(context);

            result.ShouldBeNull();
        }

        [Fact]
        public void Should_return_forbidden_response_with_RequiresValidatedClaims_enabled_but_claims_missing()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            module.RequiresValidatedClaims(s => true);
            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser("username")
                              };
            var result = module.Before.Invoke(context);

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Should_call_IsValid_delegate_with_RequiresValidatedClaims_and_valid_username()
        {
            bool called = false;
            var module = new FakeHookedModule(new BeforePipeline());
            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser("username", new[] {"Claim1", "Claim2", "Claim3"})
                              };

            module.RequiresValidatedClaims(s =>
                {
                    called = true;
                    return true;
                });

            module.Before.Invoke(context);

            called.ShouldEqual(true);
        }

        [Fact]
        public void Should_return_null_with_RequiresValidatedClaims_and_IsValid_returns_true()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser("username", new[] {"Claim1", "Claim2", "Claim3"})
                              };

            module.RequiresValidatedClaims(s => true);

            var result = module.Before.Invoke(context);

            result.ShouldBeNull();
        }

        [Fact]
        public void Should_return_forbidden_response_with_RequiresValidatedClaims_and_IsValid_returns_false()
        {
            var module = new FakeHookedModule(new BeforePipeline());
            var context = new NancyContext
                              {
                                  CurrentUser = GetFakeUser("username", new[] {"Claim1", "Claim2", "Claim3"})
                              };

            module.RequiresValidatedClaims(s => false);

            var result = module.Before.Invoke(context);

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Should_return_redirect_response_when_request_url_is_non_secure_method_is_get_and_requires_https()
        {
            // Given
            var module = new FakeHookedModule(new BeforePipeline());
            var url = GetFakeUrl(false);
            var context = new NancyContext
                              {
                                  Request = new Request("GET", url)
                              };

            module.RequiresHttps();

            // When
            var result = module.Before.Invoke(context);

            // Then
            result.ShouldNotBeNull();
            result.ShouldBeOfType<RedirectResponse>();

            url.Scheme = "https";
            result.Headers["Location"].ShouldEqual(url.ToString());
        }

        [Fact]
        public void Should_return_forbidden_response_when_request_url_is_non_secure_method_is_post_and_requires_https()
        {
            // Given
            var module = new FakeHookedModule(new BeforePipeline());
            var url = GetFakeUrl(false);
            var context = new NancyContext
                              {
                                  Request = new Request("POST", url)
                              };

            module.RequiresHttps();

            // When
            var result = module.Before.Invoke(context);

            // Then
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Should_return_forbidden_response_when_request_url_is_non_secure_method_is_delete_and_requires_https()
        {
            // Given
            var module = new FakeHookedModule(new BeforePipeline());
            var url = GetFakeUrl(false);
            var context = new NancyContext
                              {
                                  Request = new Request("DELETE", url)
                              };

            module.RequiresHttps();

            // When
            var result = module.Before.Invoke(context);

            // Then
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Should_return_forbidden_response_when_request_url_is_non_secure_method_is_get_and_requires_https_and_redirect_is_false()
        {
            // Given
            var module = new FakeHookedModule(new BeforePipeline());
            var url = GetFakeUrl(false);
            var context = new NancyContext
            {
                Request = new Request("GET", url)
            };

            module.RequiresHttps(false);

            // When
            var result = module.Before.Invoke(context);

            // Then
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Should_return_forbidden_response_when_request_url_is_non_secure_method_is_post_and_requires_https_and_redirect_is_false()
        {
            // Given
            var module = new FakeHookedModule(new BeforePipeline());
            var url = GetFakeUrl(false);
            var context = new NancyContext
            {
                Request = new Request("POST", url)
            };

            module.RequiresHttps(false);

            // When
            var result = module.Before.Invoke(context);

            // Then
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void Should_return_null_response_when_request_url_is_secure_method_is_get_and_requires_https()
        {
            // Given
            var module = new FakeHookedModule(new BeforePipeline());
            var url = GetFakeUrl(true);
            var context = new NancyContext
            {
                Request = new Request("GET", url)
            };

            module.RequiresHttps();

            // When
            var result = module.Before.Invoke(context);

            // Then
            result.ShouldBeNull();
        }

        [Fact]
        public void Should_return_null_response_when_request_url_is_secure_method_is_post_and_requires_https()
        {
            // Given
            var module = new FakeHookedModule(new BeforePipeline());
            var url = GetFakeUrl(true);
            var context = new NancyContext
            {
                Request = new Request("POST", url)
            };

            module.RequiresHttps();

            // When
            var result = module.Before.Invoke(context);

            // Then
            result.ShouldBeNull();
        }

        private static IUserIdentity GetFakeUser(string userName, IEnumerable<string> claims = null)
        {
            var ret = A.Fake<IUserIdentity>();
            ret.UserName = userName;
            ret.Claims = claims;

            return ret;
        }

        private static Url GetFakeUrl(bool https)
        {
            return new Url
                       {
                           BasePath = null,
                           Fragment = string.Empty,
                           HostName = "localhost",
                           Path = "/",
                           Port = 80,
                           Query = string.Empty,
                           Scheme = https ? "https" : "http"
                       };
        }
    }
}