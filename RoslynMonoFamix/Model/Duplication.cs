using Fame;



    [FamePackage("Dude")]
    [FameDescription("Duplication")]
    public class Duplication : Entity
    {
        [FameProperty(Name = "multiplicationInvolved") Opposite = "duplications"]
        public Multiplication multiplicationInvolved { get; set; }







    }
}