using Fame;



    [FamePackage("Dynamix")]
    [FameDescription("Event")]
    public class Event : Entity
    {
        [FameProperty(Name = "parent") Opposite = "children"]
        public Event parent { get; set; }








    }
}