
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FastSearcher {
		
	public class Note {
		private string title;
		private string content;
		private string filename;

		public static Note Load(string filename) {
			using (TextReader r = new StreamReader(filename)) {
				string title    = r.ReadLine();
				string content  = r.ReadToEnd();
				return new Note(title, filename, title + "\n" + content);
			}
		}
		
		public static Note Create(string basedir, string title) {
			string filename = Note.NoteFilename(basedir, title);
			return new Note(title, filename, title + "\n");
		}

		private static string NoteFilename(string basedir, string title) {
			string filename = title.ToLower();
        
	        Regex spaces_re = new Regex("\\s+");
	        filename = spaces_re.Replace(filename, "-");
	        
	        Regex normalize_re = new Regex("[^a-z0-9_\\-]");
	        filename = normalize_re.Replace(filename, "");
	        
	        DateTime date = DateTime.Now;
        
        	string notefilename = System.IO.Path.Combine(basedir, filename + '-' + date.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt");

			return notefilename;
		}
		
		private Note(string title, string filename, string content) {
			this.title = title;
			this.filename = filename;
			this.content = content;
		}

		public string Filename {
			get { return this.filename; }
		}
		
		public string Title {
			get { return this.title; }
		}

		public string Content {
			get { return this.content; }
			set { this.content = value; }
		}

		public DateTime Created {
			get { return File.GetCreationTime(Filename); }
		}
		public string CreatedNiceString {
			get { return ((DateTime.Today == Created.Date) ? "Today" : Created.ToString("yyyy-MM-dd HH:mm")); }
		}

		public void Save() {
			using (StreamWriter sw = new StreamWriter(Filename)) {
				sw.Write(Content);
            	sw.Flush();
			}
		}

		public bool Match(string query) {
			Regex re = new Regex(query, RegexOptions.IgnoreCase);
			return re.IsMatch(Title) || re.IsMatch(Content);
		}
	}
}
