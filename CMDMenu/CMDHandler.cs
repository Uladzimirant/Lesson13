using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMDMenu
{

    /* Class designed to provide a console interface with enterable commands
     * It implements default exit and help behavior and exception interception
     * Also there is special exception for stopping executing function with printing only message.
     */
    public class CMDHandler
    {
        private static readonly string[]
            DefaultQuitCommands = new string[] { "quit", "exit" },
            DefaultHelpCommands = new string[] { "help" };

        private bool _continueRunning = true;

        // Dictionary that maps commandName to function to run when command is called, tuple format is
        // <Function itself, list of all aliases including commandName, description>
        private IDictionary<string, Tuple<Action, List<string>, string?>> _commands = new Dictionary<string, Tuple<Action, List<string>, string?>>();

        //helper classes, inited in constructor
        private ICollection<Tuple<Action, List<string>, string?>> _customCommands;
        private ICollection<Tuple<Action, List<string>, string?>> _defaultCommands;

        //Predicate to process if no command found, must return true if processing succesful, false if not.
        //In case of false will handle as unproccessable input.
        public Predicate<string>? DefaultAction { get; set; }

        //prefix before reading line
        public string? Prefix = "> ";
        //description shown in help
        public string? Description = null;

        private void regByStrArr(string[]? c, Action a, string d)
        {
            if (c != null && c.Length > 0) RegisterCommand(c, a, d);
        }
        public CMDHandler(bool generateQuitCommands = true, bool generateHelpCommands = true)
            : this(generateQuitCommands ? DefaultQuitCommands : null,
                   generateHelpCommands ? DefaultHelpCommands : null)
        { }
        //Creates CMDHandler with predefined help and quit commands/
        //If they empty or null then it don't create that command
        public CMDHandler(string[]? quitCommands, string[]? helpCommands)
        {
            _customCommands = new List<Tuple<Action, List<string>, string?>>();
            regByStrArr(helpCommands, PrintHelp, "This message");
            regByStrArr(quitCommands, exit, "Ends program");
            _defaultCommands = _customCommands;
            _customCommands = new List<Tuple<Action, List<string>, string?>>();
            DefaultAction = null;
        }

        /* Main cycle where program will run
         * until spectial command stops it.
         * All exceptions will be intercepted and then
         * the class will await new command.
         */
        public void Run(bool showDescriptionAtBeginning = false)
        {
            _continueRunning = true;
            if (showDescriptionAtBeginning && Description != null)
            {
                Console.WriteLine(Description);
            }
            while (_continueRunning)
            {

                string inputOriginal = AskForInput(checkExitInput: false);
                string inputLowercase = inputOriginal.ToLower();
                try
                {
                    if (_commands.TryGetValue(inputLowercase, out var action))
                    {
                        action.Item1();
                    }
                    else if (DefaultAction == null || !DefaultAction(inputOriginal))
                    {
                        HandleNoCommand(inputLowercase);
                    }
                    //else ok if action successed
                }
                catch (MessageException e) { HandleMessageException(e); }
                catch (Exception e) { HandleException(e); }
            }
        }

        //Prints message if present and awaits line to enter for return
        public string AskForInput(string? message = null, bool checkExitInput = true)
        {
            if (!string.IsNullOrEmpty(message)) Console.WriteLine(message);
            string? s = "";
            while (string.IsNullOrEmpty(s?.Trim()))
            {
                if (Prefix != null) Console.Write(Prefix);
                s = Console.ReadLine();
                if (checkExitInput) ExitChecker.Check(s);
            }
            return s.Trim();
        }

        //Registers one command and it aliases if present and function that will be called by entering that command
        public void RegisterCommand(string command, Action action, string? description = null)
        {
            RegisterCommand(new string[] { command }, action, description);
        }
        public void RegisterCommand(string[] commands, Action action, string? description = null)
        {
            for (int i = 0; i < commands.Length; i++) commands[i] = commands[i].ToLower();
            var t = Tuple.Create(action, new List<string>(commands), description);
            foreach (var c in commands) _commands.Add(c, t);
            _customCommands.Add(t);
        }

        //create aliase for already existing command
        public void RegisterAlias(string newAlias, string existingCommand)
        {
            if (!_commands.ContainsKey(existingCommand)) throw new KeyNotFoundException(existingCommand + " is not registered");
            var t = _commands[existingCommand];
            _commands.Add(newAlias, t);
            t.Item2.Add(newAlias);
        }

        //Is to register in commands to be able to quit
        private void exit()
        {
            _continueRunning = false;
        }

        //prints list of commands in help
        private void printFunc(StringBuilder builder, IEnumerable<Tuple<Action, List<string>, string?>> elems)
        {
            foreach (var elem in elems)
            {
                builder.Append(" ");
                //print all command aliases
                builder.AppendJoin(", ", elem.Item2);
                //if description provided, write it
                if (elem.Item3 != null)
                {
                    builder.Append(" - ");
                    builder.Append(elem.Item3);
                }
                builder.AppendLine();
            }
        }
        public void PrintHelp()
        {
            StringBuilder builder = new StringBuilder();
            if (Description != null) builder.AppendLine(Description);
            builder.AppendLine("Avaliable commands:");
            printFunc(builder, _customCommands);
            printFunc(builder, _defaultCommands);
            Console.Write(builder.ToString());
        }

        //These 3 methods will handle diffrent situations in cmd cycle
        protected virtual void HandleNoCommand(string s)
        {
            Console.WriteLine($"Couldn't process input \"{s}\"");
        }

        protected virtual void HandleMessageException(MessageException e)
        {
            if (!string.IsNullOrEmpty(e.Message))
            {
                Console.WriteLine(e.Message);
            }
            if (e.InnerException != null)
            {
                Console.WriteLine("Exception in question:");
                Console.WriteLine(e.InnerException.ToString());
            }
        }
        protected virtual void HandleException(Exception e)
        {
            Console.WriteLine(e.ToString());
        }

    }
}
