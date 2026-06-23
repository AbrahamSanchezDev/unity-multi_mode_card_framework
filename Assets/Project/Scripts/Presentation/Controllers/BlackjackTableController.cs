using UnityEngine;
using VContainer;
using CardFramework.Core.Engines;

namespace CardFramework.UI.Controllers
{
    public class BlackjackTableController : MonoBehaviour
    {
        private BlackjackEngine _blackjackEngine;

        // VContainer automatically injects the registered transient engine here upon instantiation
        [Inject]
        public void Construct(BlackjackEngine blackjackEngine)
        {
            _blackjackEngine = blackjackEngine;
        }

        private void Start()
        {
            // _blackjackEngine.StartRound();
        }
    }
}