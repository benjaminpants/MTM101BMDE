using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.NameMenu
{
    public interface IPage
    {
        string Name { set; get; }

        IPage rootPage { get; set; }

        string[] GetDisplay(string[] curText);

        void OnEntryClick(int index);

        void OnScreenType(string typeString);
    }

    public interface IPageFolder
    {
        void GoToPage(int index);
    }

    public interface IPageButton
    {
        string InternalName { get; set; }

        string DisplayName { get; set; }

        bool Enabled { get; set; }

        void OnClick(IPage page);

        void OnType(IPage page, string key);
    }

    public class Page : IPage
    {
        public string Name { set; get; }

        public IPage rootPage { get; set; }

        IPageButton lastClickedButton;

        public Page(string name)
        {
            Name = name;
        }

        public List<IPageButton> buttons = new List<IPageButton>();

        public string[] GetDisplay(string[] curText)
        {
            for (int i = 0; i < curText.Length; i++) //using curtext length instead of button length so we dont go over
            {
                if (i >= buttons.Count)
                {
                    curText[i] = "";
                    continue;
                }
                curText[i] = buttons[i].DisplayName;
            }
            return curText;
        }

        public void OnEntryClick(int index)
        {
            if (index >= buttons.Count) return;
            lastClickedButton = buttons[index];
            if (!lastClickedButton.Enabled) return;
            lastClickedButton.OnClick(this);
        }

        public void OnScreenType(string key)
        {
            if (lastClickedButton == null) return;
            if (buttons.IndexOf(lastClickedButton) == -1)
            {
                lastClickedButton = null;
                return;
            }
            if (!lastClickedButton.Enabled) return;
            lastClickedButton.OnType(this,key);
        }
    }

    public class Button : IPageButton
    {
        public string DisplayName { get; set; } = "Button";
        public string InternalName { get; set; } = "button";
        public bool Enabled { get; set; } = true;

        protected Action<IPageButton, IPage> OnClickAction;

        protected Action<IPageButton, IPage, string> OnTypeAction;

        public Button(string internalName, string displayName, Action<IPageButton, IPage> PageAction, Action<IPageButton, IPage, string> KeyAction = null)
        {
            DisplayName = displayName;
            InternalName = internalName;
            OnClickAction = PageAction;
            OnTypeAction = KeyAction;
        }

        virtual public void OnClick(IPage page)
        {
            if (OnClickAction == null) return;
            OnClickAction(this, page);
        }

        virtual public void OnType(IPage page, string key)
        {
            if (OnTypeAction == null) return;
            OnTypeAction(this, page, key);
        }
    }

    public class StringInput : Button
    {
        public string CurrentInput = "";

        int characterLimit = 8;


        private string actualDisplayName;

        public StringInput(string internalName, string displayName, Action<IPageButton, IPage> PageAction, Action<IPageButton, IPage, string> KeyAction = null) : base(internalName, displayName, PageAction, KeyAction)
        {
            actualDisplayName = displayName;
            DisplayName = actualDisplayName.Replace("%v", CurrentInput);
        }

        public StringInput(string internalName, string displayName, Action<IPageButton, IPage> PageAction, Action<IPageButton, IPage, string> KeyAction = null, int charLimit = 8) : this(internalName, displayName, PageAction, KeyAction)
        {
            characterLimit = charLimit;
        }

        public override void OnType(IPage page, string key)
        {
            if (key.Contains("\n") || key.Contains("\r"))
            {
                base.OnClick(page);
            }
            else if (key.Contains("\b"))
            {
                CurrentInput = CurrentInput.Remove(CurrentInput.Length - 1);
            }
            else
            {
                if (key.Length != 0 && !(CurrentInput.Length + 1 > characterLimit))
                {
                    CurrentInput += key[0];
                }
            }
            DisplayName = actualDisplayName.Replace("%v",CurrentInput);
            NameMenuManager.Refresh();
            base.OnType(page, key);
        }
    }

    public class Folder : IPage, IPageFolder
    {
        public string Name { get; set; }

        public IPage DefaultPage { private set; get; }

        public int CurrentPage { private set; get; } = -1;

        public IPage rootPage { get; set; }

        public bool showReturn = false;

        private List<IPage> Pages;

        public Folder(string name, IPage defaultPage, List<IPage> initPages)
        {
            Name = name;
            DefaultPage = defaultPage;
            Pages = initPages;

        }

        public void AddPage(IPage page)
        {
            page.rootPage = this;
            Pages.Add(page);
        }

        public void ReturnToDefaultPage()
        {
            CurrentPage = -1;
        }

        public void ReturnToDefaultPage(IPageButton but, IPage page)
        {
            ReturnToDefaultPage();
        }

        public void GoToPage(IPage page)
        {
            CurrentPage = Pages.IndexOf(page);
        }

        public void GoToPage(int pageId)
        {
            if (pageId < 0 || pageId >= Pages.Count)
            {
                pageId = -1;
            }
            CurrentPage = pageId;
        }

        public string[] GetDisplay(string[] curText)
        {
            if (CurrentPage == -1)
            {
                curText = DefaultPage.GetDisplay(curText);
                if (showReturn)
                {
                    curText[curText.Length - 1] = "Return";
                }
                return curText;
            }
            return Pages[CurrentPage].GetDisplay(curText);
        }

        public void OnEntryClick(int index)
        {
            if (CurrentPage == -1) 
            {
                if (index == 7 && showReturn)
                {
                    NameMenuManager.ReturnFromPage(this);
                    return;
                }
                DefaultPage.OnEntryClick(index);
                return;
            }
            Pages[CurrentPage].OnEntryClick(index);
        }

        public void OnScreenType(string typeString)
        {
            if (CurrentPage == -1) { DefaultPage.OnScreenType(typeString); return; } //typing stuff may go through but. oh well, you have to click it anyway.
            Pages[CurrentPage].OnScreenType(typeString);
        }
    }

}
