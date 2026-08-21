namespace RentIsDue.Core
{
    using RentIsDue.Player;

    public interface IInteractable
    {
        bool CanInteract(PlayerInteractor player);
        string GetInteractionText();
        void Interact(PlayerInteractor player);
    }
}
