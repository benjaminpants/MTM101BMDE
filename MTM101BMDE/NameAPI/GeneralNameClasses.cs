using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace MTM101BaldAPI.NameMenu
{

    public class Page
    {

        public string pagename;
        public string prevpage;
        public bool manditory;
        private bool requiresconstantrefresh;
        public List<MenuObject> Elements = new List<MenuObject>();
        private Func<List<MenuObject>> refresh_func;

        public Page()
        {
            pagename = "null";
            prevpage = "root";
            manditory = false;
            requiresconstantrefresh = false;
        }

        public Page(string name, string prev, bool mand, bool refresh)
        {
            pagename = name;
            prevpage = prev;
            manditory = mand;
            requiresconstantrefresh = refresh;
        }

        public Page(string name, string prev, bool mand, bool refresh, Func<List<MenuObject>> func)
        {
            pagename = name;
            prevpage = prev;
            manditory = mand;
            requiresconstantrefresh = refresh;
            refresh_func = func;
        }


        public ref List<MenuObject> GetElements()
        {
            if (requiresconstantrefresh)
            {
                Elements = refresh_func.Invoke();
            }
            return ref Elements;
        }


    }


    public class MenuObject
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

        public MenuObject()
        {

        }

        public MenuObject(string name, string nameextern)
        {
            Name = name;
            Name_External = nameextern;
        }
        public virtual void Press()
        {

        }

    }

    public class MenuTitle : MenuObject
    {

        public MenuTitle()
        {

        }


        public MenuTitle(string intern, string name)
        {
            Name = intern;
            Name_External = name;
        }

        public override string ToString()
        {
            return Name_External;
        }

    }



    public class MenuFolder : MenuObject
    {
        public string foldertogo;

        public MenuFolder()
        {

        }


        public MenuFolder(string intern, string name, string foldertogoto)
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

    public class MenuGeneric : MenuObject
    {
        protected Action<MenuObject> function;

        public MenuGeneric()
        {

        }


        public MenuGeneric(string intern, string name, Action<MenuObject> functocall)
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

    



    public class MenuOption : MenuGeneric
    {

        private new Func<object> function;

        private object currentvalue;

        public MenuOption(string intern, string name, object def, Func<object> functocall)
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
