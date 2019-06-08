using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_Rider : IPart
	{
		public int Bonus;

		public int FeetID;

		public int OldFeetID;

		public acegiak_Rider()
		{
            
		}
        public override bool SameAs(IPart p)
		{
			return false;
		}

        public void AddBodyPart(){
            
			BodyPartType.Make("Riding", null, "riding", null, null, null, null, null, null, null, null, null, false, true);
            
            Body part = ParentObject.GetPart<Body>();
			if (part != null)
			{
				BodyPart body = part.GetBody();
				
				BodyPart bodyPart = body.AddPart("Riding");
            }

            
        }

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "ObjectCreated");
		}

		public override bool FireEvent(Event E)
		{
            if(E.ID =="ObjectCreated"){
                AddBodyPart();
            }

			return true;
		}
	}
}
