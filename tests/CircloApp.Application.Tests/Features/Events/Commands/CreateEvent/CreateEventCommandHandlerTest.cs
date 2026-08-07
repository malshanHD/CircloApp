using CircloApp.Application.Features.Events.Commands;
using CircloApp.Application.Features.Events.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using Moq;

namespace CircloApp.Application.Tests.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommandHandlerTest
    {
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IEventRepository> _eventRepositoryMock = new();
        private readonly Mock<IEventMemberRepository> _eventMemberRepositoryMock = new();

        private readonly DateTime _utcNow = new(2026, 8, 3, 12, 0, 0, DateTimeKind.Utc);

        public CreateEventCommandHandlerTest()
        {
            _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_utcNow);
        }

        [Fact]
        public async Task CreateValidEvent()
        {
            Setup();
            var handler = Handler();
            var req = CreateValidRequest();
            Guid id = Guid.NewGuid();
            var command = new CreateEventCommand(id, req);

            var response = await handler.Handle(command, CancellationToken.None);

            _eventRepositoryMock.Verify(x => x.AddAsync(It.IsAny<BudgetEvent>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        private CreateEventCommandHandler Handler()
        {
            return new CreateEventCommandHandler(
                _eventRepositoryMock.Object, _eventMemberRepositoryMock.Object, _unitOfWorkMock.Object, _dateTimeProviderMock.Object
            );
        }

        private static CreateEventRequest CreateValidRequest() => new()
        {
            Name = "Ella Trip",
            Description = "Ella Hiking"
        };

        private void Setup()
        {
            _eventRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BudgetEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        }
    }
}
