using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lesson13
{
    [Serializable]
    public class Squad
    {
        public string SquadName { get; set; }
        public string HomeTown { get; set; }
        public int Formed { get; set; }
        public string SecretBase { get; set; }
        public bool Active { get; set; }
        public List<Member> Members { get;  set; }



        public override string? ToString() =>
            string.Join(Environment.NewLine, new string[]
            {
                $"Squad \"{SquadName}\":",
                $"Home town - {HomeTown}, Formed - {Formed}, Secret base - {SecretBase}, {(Active ? "Active" : "Not active")}",
                "Members:",
                string.Join(Environment.NewLine, Members)
            });
    }

    [Serializable]
    public class Member
    {
        public string Name { get; set; }

        private int _age;
        public int Age { get => _age;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Must be not negative");
                _age = value;
            }
        }
        public string SecretIdentity { get; set; }
        public List<string> Powers { get; set; }


        public override string? ToString() =>
        string.Join(Environment.NewLine,
            new string[] {
                $"{Name}: Age - {Age}, Secret Identity - {SecretIdentity}",
                $"   Powers: ({string.Join(", ", Powers)})"
            });
  
    }
}
