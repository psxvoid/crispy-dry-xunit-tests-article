namespace Core;

public interface IDecision
{
    public bool IsPlayerHasControl { get; }
}

public interface IBodyController
{
    void Jump();
}

public interface IArtificialIntelligence
{
    IDecision NextDecision();
}

public class IdleDecision : IDecision
{
    public bool IsPlayerHasControl => true;
}

public class MarioPossessedHero
{
    private readonly IArtificialIntelligence artificialIntelligence;
    private readonly IBodyController bodyController;

    private IDecision activeAction = new IdleDecision();

    public MarioPossessedHero(IArtificialIntelligence artificialIntelligence, IBodyController bodyController)
    {
        artificialIntelligence = artificialIntelligence ?? throw new ArgumentNullException(nameof(artificialIntelligence));
        bodyController = bodyController ?? throw new ArgumentNullException(nameof(bodyController));

        this.artificialIntelligence = artificialIntelligence;
        this.bodyController = bodyController;
    }

    public void DecideWhatToDo()
    {
        this.activeAction = this.artificialIntelligence.NextDecision();
    }

    public void Jump()
    {
        if (this.activeAction.IsPlayerHasControl)
        {
            this.bodyController.Jump();
        }
    }
}