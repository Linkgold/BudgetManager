using Application.Interfaces;

namespace Tests.API.Fixtures
{
    public class FakeCurrentUserService : ICurrentUserService
    {
        private readonly ApiTestFixture _fixture;

        public int UserId => _fixture.TestUserId;
        public bool IsAuthenticated => true;
        public string? UserName => "TestUser";
        public string? Email => "test@email.com";

        public FakeCurrentUserService(ApiTestFixture fixture) => _fixture = fixture;
    }
}