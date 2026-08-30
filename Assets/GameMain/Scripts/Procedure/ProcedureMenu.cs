using GameFramework.Fsm;
using GameFramework.Procedure;
using SepCore.UI;

namespace SepCore.Procedure
{
    public class ProcedureMenu : ProcedureBase
    {
        protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            DialogParams context = new DialogParams()
            {
                Message = "This is a test dialog.",
                Title = "This is a test dialog.",
                Mode = 2,
                ConfirmText = "确认",
                CancelText = "取消",
            };
            GameEntry.UI.OpenDialog(context);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnDestroy(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }
    }
}
