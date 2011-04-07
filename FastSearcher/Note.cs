
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
				string content  = r.ReadToEnd();
				FileSystemInfo info = new FileInfo(filename);
				string title = info.Name;
				return new Note(title, filename, content);
			}
		}
		
		public static Note Create(string basedir, string title) {
			string filename = Note.NoteFilename(basedir, title);
			return new Note(title, filename, "");
		}

		private static string NoteFilename(string basedir, string title) {
			return basedir + "/" + title;
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
			get {
				return (
					(DateTime.Today == Created.Date) 
				        ? "Today " + Created.ToString("HH:mm")
				        : Created.ToString("yyyy-MM-dd HH:mm")
				        ); 
			}
		}

		public void Save() {
			using (StreamWriter sw = new StreamWriter(Filename)) {
				sw.Write(Content);
            	sw.Flush();
			}
		}

		public bool Match(string query) {
			Regex re = new Regex(query, RegexOptions.IgnoreCase);
			return re.IsMatch(Title) || re.IsMatch(Filename) || re.IsMatch(Content);
		}
	}
}
