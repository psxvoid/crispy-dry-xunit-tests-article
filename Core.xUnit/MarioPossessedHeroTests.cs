namespace Core.xUnit;

using Core;
using Xunit;
using Moq;

public static class MarioPossessedHeroTests
{
    public abstract class BeforeAllFixture<TSut>
    {
        protected BeforeAllFixture()
        {
        }

        public TSut? Sut { get; protected set; }

        public object? Result { get; protected set; }

        public Exception? SutCreationException { get; protected set; }
        public Exception? ActException { get; protected set; }

        public abstract TSut? CreateSutFunc();

        public abstract object? ActFunc();

        public void CreateSut()
        {
            this.SutCreationException = Record.Exception(() => this.Sut = this.CreateSutFunc());
        }

        public void Act()
        {
            this.ActException = Record.Exception(() => this.Result = this.ActFunc());
        }
    }

    public abstract class BeforeAll : BeforeAllFixture<MarioPossessedHero>
    {
        public Mock<IArtificialIntelligence>? ArtificialIntelligence { get; protected set; }

        public Mock<IBodyController>? BodyController { get; protected set; }

        public override MarioPossessedHero CreateSutFunc()
        {
            return new MarioPossessedHero(
                this.ArtificialIntelligence?.Object!,
                this.BodyController?.Object!
            );
        }
    }

    public static class Constructor
    {
        public abstract class BeforeAll : MarioPossessedHeroTests.BeforeAll
        {
            protected BeforeAll()
            {
            }

            public override object? ActFunc()
            {
                return null;
            }
        }

        public class NullParameters : IClassFixture<NullParameters.BeforeAll>
        {
            private readonly BeforeAll fixture;

            public class BeforeAll : Constructor.BeforeAll
            {
                public BeforeAll()
                {
                    this.ArtificialIntelligence = null;
                    this.BodyController = null;

                    this.CreateSut(); // act for the constructor
                }
            }

            public NullParameters(BeforeAll fixture)
            {
                this.fixture = fixture;
            }

            [Fact]
            public void Throws()
            {
                Assert.IsType<ArgumentNullException>(this.fixture.SutCreationException);
                Assert.Contains("artificialIntelligence", this.fixture.SutCreationException.Message, StringComparison.InvariantCulture);
            }

            [Fact]
            public void ShouldNotCreateSut()
            {
                Assert.Null(this.fixture.Sut);
            }
        }

        public class SecondParamIsNull : IClassFixture<SecondParamIsNull.BeforeAll>
        {
            private readonly BeforeAll fixture;

            public class BeforeAll : Constructor.BeforeAll
            {
                public BeforeAll()
                {
                    this.ArtificialIntelligence = new Mock<IArtificialIntelligence>();
                    this.BodyController = null;

                    this.CreateSut(); // act for the constructor
                }
            }

            public SecondParamIsNull(BeforeAll fixture)
            {
                this.fixture = fixture;
            }

            [Fact]
            public void Throws()
            {
                Assert.IsType<ArgumentNullException>(this.fixture.SutCreationException);
                Assert.Contains("bodyController", this.fixture.SutCreationException.Message, StringComparison.InvariantCulture);
            }

            [Fact]
            public void ShouldNotCreateSut()
            {
                Assert.Null(this.fixture.Sut);
            }
        }

        public class NotNullableParams : IClassFixture<NotNullableParams.BeforeAll>
        {
            private readonly BeforeAll fixture;

            public class BeforeAll : Constructor.BeforeAll
            {
                public BeforeAll()
                {
                    this.ArtificialIntelligence = new Mock<IArtificialIntelligence>();
                    this.BodyController = new Mock<IBodyController>();

                    this.CreateSut(); // act for the constructor
                }
            }

            public NotNullableParams(BeforeAll fixture)
            {
                this.fixture = fixture;
            }

            [Fact]
            public void ShouldNotThrows()
            {
                Assert.Null(this.fixture.SutCreationException);
            }

            [Fact]
            public void ShouldNotCreateSut()
            {
                Assert.NotNull(this.fixture.Sut);
            }
        }
    }

    public static class Methods
    {
        public abstract class BeforeAll : Constructor.NotNullableParams.BeforeAll
        {
        }

        public static class Jump
        {
            public abstract class BeforeAll : Methods.BeforeAll
            {
                public Mock<IDecision> Decision { get; protected set; }
                public bool IsPlayerHasControl { get; protected set; }

                protected BeforeAll()
                {
                    this.Decision = new Mock<IDecision>();
                    this.Decision.SetupGet(x => x.IsPlayerHasControl).Returns(() => this.IsPlayerHasControl);

                    this.ArtificialIntelligence!.Setup(x => x.NextDecision()).Returns(() => this.Decision.Object);
                }

                public override object? ActFunc()
                {
                    this.Sut!.Jump();

                    return null;
                }
            }

            public static class AfterAiMadeDecision
            {
                public abstract class BeforeAll : Jump.BeforeAll
                {
                    protected BeforeAll()
                    {
                        this.Sut!.DecideWhatToDo(); // update the decision from AI
                    }
                }

                public class PlayerHasControl : IClassFixture<PlayerHasControl.BeforeAll>
                {
                    private readonly BeforeAll fixture;

                    public class BeforeAll : AfterAiMadeDecision.BeforeAll
                    {
                        public BeforeAll()
                        {
                            this.IsPlayerHasControl = true;
                            this.Act();
                        }
                    }

                    public PlayerHasControl(BeforeAll fixture)
                    {
                        this.fixture = fixture;
                    }

                    [Fact]
                    public void ShouldNotThrow()
                    {
                        Assert.Null(this.fixture.ActException);
                    }

                    [Fact]
                    public void ShouldTriggerJumpForBodyController()
                    {
                        this.fixture.BodyController!.Verify(x => x.Jump(), Times.Once);
                    }
                }

                public class PlayerHasNoControl : IClassFixture<PlayerHasNoControl.BeforeAll>
                {
                    private readonly BeforeAll fixture;

                    public class BeforeAll : AfterAiMadeDecision.BeforeAll
                    {
                        public BeforeAll()
                        {
                            this.IsPlayerHasControl = false;
                            this.Act();
                        }
                    }

                    public PlayerHasNoControl(BeforeAll fixture)
                    {
                        this.fixture = fixture;
                    }

                    [Fact]
                    public void ShouldNotThrow()
                    {
                        Assert.Null(this.fixture.ActException);
                    }

                    [Fact]
                    public void ShouldNotTriggerJumpForBodyController()
                    {
                        this.fixture.BodyController!.Verify(x => x.Jump(), Times.Never);
                    }
                }

                public class ExceptionDuringCheckingPlayerControl : IClassFixture<ExceptionDuringCheckingPlayerControl.BeforeAll>
                {
                    private readonly BeforeAll fixture;

                    public class BeforeAll : AfterAiMadeDecision.BeforeAll
                    {
                        public BeforeAll()
                        {
                            this.Decision
                                .SetupGet(x => x.IsPlayerHasControl)
                                .Throws(new Exception("Player control exception."));
                            this.Act();
                        }
                    }

                    public ExceptionDuringCheckingPlayerControl(BeforeAll fixture)
                    {
                        this.fixture = fixture;
                    }

                    [Fact]
                    public void ShouldThrow()
                    {
                        Assert.NotNull(this.fixture.ActException);
                        Assert.Equal("Player control exception.", this.fixture.ActException.Message);
                    }

                    [Fact]
                    public void ShouldNotTriggerJumpForBodyController()
                    {
                        this.fixture.BodyController!.Verify(x => x.Jump(), Times.Never);
                    }
                }
            }
        }
    }
}
