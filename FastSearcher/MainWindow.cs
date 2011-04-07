using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Gtk;

using FastSearcher;

public partial class MainWindow: Gtk.Window
{
    private Gtk.ListStore       filename_list;
    private Gtk.TreeModelFilter filter;
    private string basedir = "/home/peter/notes";

    private bool saving_allowed = false;
    private bool verbose = false;

    private NoteStore note_store;
    private Note      current_note;
        
    public MainWindow (): base (Gtk.WindowType.Toplevel)
    {
        Build();
        note_store = new NoteStore(basedir);
        
        preview.ModifyFont(Pango.FontDescription.FromString("Monospace 12"));
        
        Gtk.TreeViewColumn titleColumn = new Gtk.TreeViewColumn ();
        titleColumn.Title = "Title";
        Gtk.CellRendererText titleCell = new Gtk.CellRendererText();
        titleColumn.PackStart(titleCell, true);
        titleColumn.AddAttribute(titleCell, "text", 0);
        titleColumn.SortColumnId = 0;
        titleColumn.Expand = true;
        filenames.AppendColumn(titleColumn);
/*
        if (false) {
            Gtk.TreeViewColumn filenameColumn = new Gtk.TreeViewColumn ();
            filenameColumn.Title = "Filename";
            Gtk.CellRendererText filenameCell = new Gtk.CellRendererText();
            filenameColumn.PackStart(filenameCell, true);
            filenameColumn.AddAttribute(filenameCell, "text", 1);
            filenameColumn.SortColumnId = 1;
            filenames.AppendColumn(filenameColumn);
        }
*/

        Gtk.TreeViewColumn dateColumn = new Gtk.TreeViewColumn ();        
        dateColumn.Title = "Date added";
        Gtk.CellRendererText dateCell = new Gtk.CellRendererText();
        dateColumn.PackStart(dateCell, true);
        dateColumn.SetCellDataFunc(dateCell, this.RenderDate);
        dateColumn.SortColumnId = 2;
        
        filenames.AppendColumn(dateColumn);
                
        filename_list = new ListStore(typeof(String), typeof(String), typeof(DateTime), typeof(Note));
        UpdateFiles();
        
        filename_list.SetSortColumnId(0, SortType.Ascending);
        
        filter = new Gtk.TreeModelFilter (filename_list, null);

        filter.VisibleFunc = new TreeModelFilterVisibleFunc(FilterTree);

        TreeModelSort tm = new TreeModelSort(filter);
        tm.SetSortFunc(2, this.SortDates);
        filenames.Model = tm;

        preview.WrapMode = WrapMode.Word;
        preview.ModifyFont(Pango.FontDescription.FromString("Droid Sans Mono 10"));
        preview.Buffer.Changed += new EventHandler(this.WriteToNotefile);
    }                                      

    int SortDates(TreeModel model, TreeIter a, TreeIter b) {
        DateTime date_a = (DateTime)model.GetValue(a, 2);
        DateTime date_b = (DateTime)model.GetValue(b, 2);
        return date_a.CompareTo(date_b);
    }
    
    void RenderDate(Gtk.TreeViewColumn _column, Gtk.CellRenderer _cell, Gtk.TreeModel _model, Gtk.TreeIter _iter) {
        Note note = _model.GetValue(_iter, 3) as Note;
        CellRendererText c = _cell as Gtk.CellRendererText; 
        c.Text = note.CreatedNiceString;
    }

    public bool FilterTree(Gtk.TreeModel model, Gtk.TreeIter iter) {
        object val = model.GetValue(iter, 3);
        if (val == null) {
            return false;
        }
        Note note = val as Note;
        return note.Match(search.Text);
    }
    
    protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
        Application.Quit();
        a.RetVal = true;
    }
    
    public void UpdateFiles() {
        filename_list.Clear();

        List<Note> notes = note_store.Notes();

        foreach (Note note in notes) {
            filename_list.AppendValues(note.Title, note.Filename, note.Created, note);
        }
    }

    protected virtual void OnRowActivated (object o, Gtk.RowActivatedArgs args) {
        TreeIter iter;
        if (filenames.Model.GetIterFromString(out iter, args.Path.ToString())) {
            string filename = filenames.Model.GetValue(iter, 1).ToString();
            if (verbose) {
                Console.WriteLine("RowActivated " + filename);
            }
            SetCurrentNote(filename);
            preview.GrabFocus();
        }
    }

    protected virtual void OnFilenamesCursorChanged (object sender, System.EventArgs e) {
        TreeIter iter;
        
        if (filenames.Selection.GetSelected(out iter)) {
            string filename = filenames.Model.GetValue(iter, 1).ToString();
            SetCurrentNote(filename);
        }
    }

    void SetCurrentNote(string filename) {
        saving_allowed = false;
        
        Note note           = Note.Load(filename);
        current_note        = note;
        preview.Buffer.Text = note.Content;
        
        saving_allowed = true;
    }
    
    void SetCurrentNote(Note note) {
        saving_allowed = false;
        
        current_note        = note;
        preview.Buffer.Text = note.Content;
        
        saving_allowed = true;
    }

    void WriteToNotefile(object o, System.EventArgs args) {
        if (!saving_allowed) {
            if (verbose) {
                Console.WriteLine("Tried to save, but ignored: saving == " + saving_allowed);
            }
            return;
        }
        
        if (verbose) {
            Console.WriteLine("Writing " + preview.Buffer.LineCount + " lines to " + current_note.Filename);
        }

        current_note.Content = preview.Buffer.Text;
        current_note.Save();
    }
    
    protected virtual void OnFilterEntryTextChanged (object sender, System.EventArgs e)
    {
        filter.Refilter();
    }

    public Note CreateNewNote() {
        Note note = Note.Create(basedir, search.Text);
        note.Save();
        
        if (verbose) {
            Console.WriteLine("Created new note: " + note.Filename);
        }
        
        return note;
    }

    protected virtual void OnFilterEntryActivated (object sender, System.EventArgs e) {
        if (filenames.Model.IterNChildren() == 0) {
            Note note = CreateNewNote();
            UpdateFiles();
            filter.Refilter();
            SetCurrentNote(note);
            preview.GrabFocus();
        }
        else if (filenames.Model.IterNChildren() == 1) {
            // Select note
            TreeIter iter;
            if (filenames.Model.IterChildren(out iter)) {
                string filename = filenames.Model.GetValue(iter, 1).ToString();
                SetCurrentNote(filename);
                preview.GrabFocus();
            }
        }
        else {
            filenames.GrabFocus();
        }
    }

    protected virtual void OnFilenamesKeyPressEvent (object o, Gtk.KeyPressEventArgs args) {
        if (args.Event.Key == Gdk.Key.Escape) {
            search.GrabFocus();
        }
        else if ((args.Event.State & Gdk.ModifierType.Mod1Mask) != 0) {
            switch(args.Event.Key) {
                case Gdk.Key.j:
                case Gdk.Key.J: {
                    TreePath path;
                    TreeViewColumn column;
                    filenames.GetCursor(out path, out column);
                    if (path != null) {
                        path.Next();
                        TreeIter iter;
                        if (filenames.Model.GetIter(out iter, path)) {
                            filenames.SetCursor(path, column, false);
                        }
                    }
                    break;
                }
                case Gdk.Key.k:
                case Gdk.Key.K: {
                    TreePath path;
                    TreeViewColumn column;
                    filenames.GetCursor(out path, out column);
                    if (path != null) {
                        path.Prev();
                        filenames.SetCursor(path, column, false);
                    }                   
                    break;
                }
            }
        }
    }

    protected virtual void OnSearchKeyPressEvent (object o, Gtk.KeyPressEventArgs args) {
        if (args.Event.Key == Gdk.Key.Escape) {
            if (search.Text.Length > 0) {
                search.Text = "";
            }
            else {
                Gtk.Main.Quit();
            }
        }
        else if ((args.Event.State & Gdk.ModifierType.Mod1Mask) != 0) {
            switch (args.Event.Key) {
            case Gdk.Key.J:
            case Gdk.Key.j:
                filenames.GrabFocus();
                
                /* Select the first item in the list. */
                TreePath path;
                TreeViewColumn column;
                filenames.GetCursor(out path, out column);
                if (path != null) {
                    TreeIter iter;
                    if (filenames.Model.GetIter(out iter, path)) {
                        filenames.SetCursor(path,column, false);
                    }
                }
                break;
            }
        }
    }

    protected virtual void OnPreviewKeyPressEvent (object o, Gtk.KeyPressEventArgs args) {
        switch(args.Event.Key) {
        case Gdk.Key.Escape:
            search.GrabFocus();
            break;
        }
    }

    protected virtual void OnAboutActionActivated (object sender, System.EventArgs e) {
        FastSearcher.AboutDialog d = new FastSearcher.AboutDialog();
        d.Show();
    }

    protected virtual void OnQuitActionActivated (object sender, System.EventArgs e) {
        Gtk.Main.Quit();
    }
}

