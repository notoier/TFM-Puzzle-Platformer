public interface IActivable
{
    bool IsActive { get; }
    
    public void Activate();
    public void Deactivate();
}
