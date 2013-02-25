
using EnvDTE;
namespace S2.STools.Commands
{
    interface ICommand
    {
        bool IsEnable(DTE dte);
        void Execute(DTE dte);
        bool IsYourId(uint commandId);
    }
}
