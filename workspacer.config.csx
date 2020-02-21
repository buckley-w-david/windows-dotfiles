#r "C:\Program Files\workspacer\workspacer.Shared.dll"
#r "C:\Program Files\workspacer\plugins\workspacer.Bar\workspacer.Bar.dll"
#r "C:\Program Files\workspacer\plugins\workspacer.ActionMenu\workspacer.ActionMenu.dll"
#r "C:\Program Files\workspacer\plugins\workspacer.FocusIndicator\workspacer.FocusIndicator.dll"

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using workspacer;
using workspacer.Bar;
using workspacer.Bar.Widgets;
using workspacer.ActionMenu;
using workspacer.FocusIndicator;

private ActionMenuItemBuilder CreateRemoveWorkspaceMenu(IWorkspaceContainer container, ActionMenuPlugin actionMenu)
{
    var menu = actionMenu.Create();
    foreach (var w in container.GetAllWorkspaces())
    {
        menu.Add(w.Name, () => container.RemoveWorkspace(w));
    }
    return menu;
}

private void SwitchToWorkspace(IConfigContext context, string name) {
    var container = context.WorkspaceContainer;
    var w = container[name];
    if (w == null) {
        context.WorkspaceContainer.CreateWorkspace(name);
    }
    w = container[name];
    context.Workspaces.SwitchToWorkspace(w);
}

Action<IConfigContext> doConfig = (context) => {
    var mod = workspacer.KeyModifiers.LAlt;
    var monitors = context.MonitorContainer.GetAllMonitors();

    var titleWidget = new TitleWidget();
    titleWidget.MonitorHasFocusColor = Color.Red;

    context.AddBar(new BarPluginConfig() {
        BarTitle = "workspacer.Bar",
        BarHeight = 20,
        FontSize = 9,
        DefaultWidgetForeground = Color.White,
        DefaultWidgetBackground = Color.Black,
        Background = Color.Black,
        LeftWidgets = () => new IBarWidget[] { new WorkspaceWidget(),  new ActiveLayoutWidget(), new TextWidget("    "), titleWidget },
        RightWidgets = () => new IBarWidget[] { new TimeWidget(1000, "ddd, M/dd/yyyy | h:mm tt") },
    });

    context.AddFocusIndicator(new workspacer.FocusIndicator.FocusIndicatorPluginConfig() {
        BorderSize = 2,
    });

    var actionMenu = context.AddActionMenu(new workspacer.ActionMenu.ActionMenuPluginConfig() {
        MenuWidth = 960,
        FontSize = 10,
    });
    var menu = actionMenu.Create();

    context.DefaultLayouts = () => new ILayoutEngine[] { new FullLayoutEngine(), new TallLayoutEngine() };

    var workspaceNames = new List<string> {
         "comms", "main", "docs", "dev1", "dev2"
     };
    context.WorkspaceContainer.CreateWorkspaces(workspaceNames.Select((name, index) => string.Concat(index + 1, ": ", name)).ToArray());

    context.WindowRouter.AddRoute((window) => window.ProcessName.Equals("slack") || window.ProcessName.Equals("outlook") ? context.WorkspaceContainer["comms"] : null);

    var container = context.WorkspaceContainer;
    menu.AddFreeForm("create workspace", (s) => container.CreateWorkspace(s));
    menu.AddFreeForm("switch to workspace", (s) => SwitchToWorkspace(context, s));
    menu.AddMenu("remove workspace", CreateRemoveWorkspaceMenu(container, actionMenu));
    // context.Keybinds.Subscribe(mod, Keys.Back, () => container.RemoveWorkspace();

    context.Keybinds.Subscribe(mod, Keys.V, () => actionMenu.ShowFreeForm("Workspace Name", (s) => SwitchToWorkspace(context, s)), "switch to workspace");
    context.Keybinds.Subscribe(mod, Keys.R, () => context.Restart(), "restart workspacer");
    context.Keybinds.Subscribe(mod, Keys.Q, () => context.Quit(), "quit workspacer");
    // context.Keybinds.Subscribe(mod | KeyModifiers.LControl, Keys.Enter, () => sp);
    context.Keybinds.Subscribe(mod, Keys.P, () => actionMenu.ShowMenu(menu), "open action menu");
};
return doConfig;
