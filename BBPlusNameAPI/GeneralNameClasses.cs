using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace BBPlusNameAPI
{

    public class Name_Page
    {

        public string pagename;
        public string prevpage;
        public bool manditory;
        private bool requiresconstantrefresh;
        public List<Name_MenuObject> Elements = new List<Name_MenuObject>();
        private Func<List<Name_MenuObject>> refresh_func;

        public Name_Page()
        {
            pagename = "null";
            prevpage = "root";
            manditory = false;
            requiresconstantrefresh = false;
        }

        public Name_Page(string name, string prev, bool mand, bool refresh)
        {
            pagename = name;
            prevpage = prev;
            manditory = mand;
            requiresconstantrefresh = refresh;
        }

        public Name_Page(string name, string prev, bool mand, bool refresh, Func<List<Name_MenuObject>> func)
        {
            pagename = name;
            prevpage = prev;
            manditory = mand;
            requiresconstantrefresh = refresh;
            refresh_func = func;
        }


        public ref List<Name_MenuObject> GetElements()
        {
            if (requiresconstantrefresh)
            {
                Elements = refresh_func.Invoke();
            }
            return ref Elements;
        }


    }


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
        protected Action<Name_MenuObject> function;

        public Name_MenuGeneric()
        {

        }


        public Name_MenuGeneric(string intern, string name, Action<Name_MenuObject> functocall)
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
            function.Invoke(this);
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
