using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    class KEES_PEC : PartModule
    {

        [KSPField(isPersistant = false)]
        public string nodeName;

        private AttachNode node;
        private KEESExperiment exp = null;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            node = part.findAttachNode(nodeName);
            if (node == null)
            {
                NE_Helper.logError("KEES PEC: AttachNode not found");
                node = part.attachNodes.First();
            }

            StartCoroutine(checkNode());
        }

        public System.Collections.IEnumerator checkNode()
        {
            while (true)
            {
                checkForExp();
                yield return new UnityEngine.WaitForSeconds(1f);
            }
        }

        private void checkForExp()
        {
            if (node != null && node.attachedPart != null)
            {
                KEESExperiment newExp = node.attachedPart.GetComponent<KEESExperiment>();
                if (newExp != null)
                {
                    if (exp != null && exp != newExp)
                    {
                        exp.dockedToPEC(false);
                        exp = newExp;
                        exp.dockedToPEC(true);
                    }
                    else if (exp == null)
                    {
                        exp = newExp;
                        exp.dockedToPEC(true);
                    }
                    return;
                }
            }
            if (exp != null)
            {
                exp.dockedToPEC(false);
                exp = null;
            }
        }

    }
}
