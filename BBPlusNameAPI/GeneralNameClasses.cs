using System;

namespace BBPlusNameAPI
{

    public class Name_MenuObject
    {
        public string Name;
        protected string Name_External;

        public override string ToString()
        {
            return Name_External;
        }

        public virtual string GetName()
        {
            return Name_External;
        }

        public Name_MenuObject()
        {

        }

        public Name_MenuObject(string name, string nameextern)
        {
            Name = name;
            Name_External = nameextern;
        }
        public virtual void Press()
        {

        }

    }

    public class Name_MenuTitle : Name_MenuObject
    {

        public Name_MenuTitle()
        {

        }


        public Name_MenuTitle(string intern, string name)
        {
            Name = intern;
            Name_External = name;
        }

        public override string ToString()
        {
            return Name_External;
        }

    }



    public class Name_MenuFolder : Name_MenuObject
    {
        public string foldertogo;

        public Name_MenuFolder()
        {

        }


        public Name_MenuFolder(string intern, string name, string foldertogoto)
        {
            Name = intern;
            Name_External = name;
            foldertogo = foldertogoto;
        }

        public override string ToString()
        {
            return Name_External;
        }

        public override void Press()
        {
            NameMenuManager.Current_Page = foldertogo;
        }

    }

    public class Name_MenuGeneric : Name_MenuObject
    {
        protected Action function;

        public Name_MenuGeneric()
        {

        }


        public Name_MenuGeneric(string intern, string name, Action functocall)
        {
            Name = intern;
            Name_External = name;
            function = functocall;
        }

        public override string ToString()
        {
            return Name_External;
        }

        public override void Press()
        {
            function.Invoke();
        }

    }



    public class Name_MenuOption : Name_MenuGeneric
    {

        private new Func<object> function;

        private object currentvalue;

        public Name_MenuOption(string intern, string name, object def, Func<object> functocall)
        {
            Name = intern;
            Name_External = name;
            function = functocall;
            currentvalue = def;
        }

        public override string GetName()
        {
            return Name_External + ": " + currentvalue.ToString();
        }

        public override void Press()
        {
            currentvalue = function.Invoke();
        }

    }
}
