using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbExpressions
{
    class KerbalActor
    {
        public Component Component { get; private set; }
        public ProtoCrewMember CrewMember { get; private set; }
        public KerbalEVA EVA { get; private set; }
        public kerbalExpressionSystem ExpressionSystem { get; private set; }

        public KerbalActor(Component component, ProtoCrewMember crewMember)
        {
            if (component == null)
            {
                throw new ArgumentNullException("behavior");
            }
            if (crewMember == null)
            {
                throw new ArgumentNullException("crewMember");
            }

            Util.Log("Creating actor: {0} with name {1}, gameobject name {2}", crewMember.name, component.name, component.gameObject.name);

            Component = component;

            Util.PrintGameObjectBehaviors(component.gameObject);
            CrewMember = crewMember;

            EVA = component.GetComponent<KerbalEVA>();
            if (EVA == null)
            {
                throw new InvalidOperationException("Component does not have KerbalEVA");
            }

            ExpressionSystem = component.GetComponent<kerbalExpressionSystem>();
            if (ExpressionSystem != null)
            {
                Util.Log("Found Expression System");
                //throw new InvalidOperationException("Component does not have ExpressionAI");
            }
        }
    }
}
