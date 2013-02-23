
namespace S2.STools.Commands
{
    interface ICommand
    {
        bool IsEnable();
        void Execute();
        bool IsYourId(uint commandId);
    }
}
