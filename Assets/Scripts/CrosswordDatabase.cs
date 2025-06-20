using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class CrosswordDatabase
{
    public List<CrosswordEntry> Entries = new List<CrosswordEntry>();
}

public class OriginalDatabase
{
    public List<CrosswordEntry> crosswordEntries = new List<CrosswordEntry>
    {
        new CrosswordEntry("A hot drink made from roasted beans", "COFFEE"),
        new CrosswordEntry("A yellow citrus fruit", "LEMON"),
        new CrosswordEntry("A small bed for a baby", "CRIB"),
        new CrosswordEntry("The color of grass", "GREEN"),
        new CrosswordEntry("The month after December", "JANUARY"),
        new CrosswordEntry("The liquid we drink to stay hydrated", "WATER"),
        new CrosswordEntry("What you wear on your feet", "SHOES"),
        new CrosswordEntry("An animal known for its long neck", "GIRAFFE"),
        new CrosswordEntry("The opposite of 'up'", "DOWN"),
        new CrosswordEntry("A well-known company that makes phones", "APPLE"),
        new CrosswordEntry("The sound a dog makes", "BARK"),
        new CrosswordEntry("A day of the week that starts with 'M'", "MONDAY"),
  
        new CrosswordEntry("A soft, white mineral used in pencils", "GRAPHITE"),
        new CrosswordEntry("A metal used to make coins", "COPPER"),
        new CrosswordEntry("A meal typically eaten in the morning", "BREAKFAST"),
        new CrosswordEntry("A small, winged insect that sucks blood", "MOSQUITO"),
        new CrosswordEntry("The seventh month of the year", "JULY"),
        new CrosswordEntry("A tool used to hit nails", "HAMMER"),
        new CrosswordEntry("The color of the sky on a clear day", "BLUE"),
        new CrosswordEntry("A large body of saltwater", "OCEAN"),
        new CrosswordEntry("A vehicle with two wheels", "BICYCLE"),
        new CrosswordEntry("The part of a plant that absorbs water", "ROOT"),
        new CrosswordEntry("The opposite of 'start'", "STOP"),
        new CrosswordEntry("The largest planet in our solar system", "JUPITER"),
        new CrosswordEntry("The color of coal", "BLACK"),
        new CrosswordEntry("A piece of furniture for sitting", "CHAIR"),
        new CrosswordEntry("A fruit that's long and yellow", "BANANA"),
        new CrosswordEntry("An animal known for its spots", "LEOPARD"),
        new CrosswordEntry("A day of the week that starts with 'S'", "SUNDAY"),
        new CrosswordEntry("A place where planes take off and land", "AIRPORT"),
        new CrosswordEntry("A machine used to wash clothes", "WASHER"),
        new CrosswordEntry("A game played with a bat and ball", "BASEBALL"),
        new CrosswordEntry("A holiday celebrated on December 25th", "CHRISTMAS"),
        new CrosswordEntry("The organ used for thinking", "BRAIN"),
        new CrosswordEntry("A place where you go to learn", "SCHOOL"),
        new CrosswordEntry("The smallest US coin", "DIME"),
        new CrosswordEntry("A flying vehicle with wings", "AIRPLANE"),
        new CrosswordEntry("The opposite of 'short'", "TALL"),
        new CrosswordEntry("The opposite of 'fast'", "SLOW"),
        new CrosswordEntry("An object used to tell time", "CLOCK"),
        new CrosswordEntry("A shape with three sides", "TRIANGLE"),
        new CrosswordEntry("A flat, round object used for eating", "PLATE"),
        new CrosswordEntry("The opposite of 'big'", "SMALL"),
        new CrosswordEntry("An animal known for its stripes", "ZEBRA"),
        new CrosswordEntry("A fruit with a hard shell and white inside", "COCONUT"),
        new CrosswordEntry("A hot, dry region with sand", "DESERT"),
        new CrosswordEntry("A fruit that is often made into raisins", "GRAPE"),
        new CrosswordEntry("The organ used to pump blood", "HEART"),
        new CrosswordEntry("The opposite of 'push'", "PULL"),
        new CrosswordEntry("The opposite of 'light'", "DARK"),

        new CrosswordEntry("An animal known for building dams", "BEAVER"),

        new CrosswordEntry("A person who delivers mail", "POSTMAN"),
        new CrosswordEntry("A shape with four equal sides", "SQUARE"),
        new CrosswordEntry("A fruit that's green and sour", "LIME"),
        new CrosswordEntry("The main ingredient in pasta", "WHEAT"),
        new CrosswordEntry("An animal known for its humps", "CAMEL"),
        new CrosswordEntry("The opposite of 'right'", "LEFT"),
        new CrosswordEntry("A place where you can borrow books", "LIBRARY"),
        new CrosswordEntry("An animal that quacks", "DUCK"),
        new CrosswordEntry("A shape with eight sides", "OCTAGON"),
        new CrosswordEntry("A round object used in many sports", "BALL"),
        new CrosswordEntry("A place where you live", "HOUSE"),
        new CrosswordEntry("A light source in the sky at night", "MOON"),
        new CrosswordEntry("An animal known for its shell", "TURTLE"),
        new CrosswordEntry("A piece of jewelry worn on the finger", "RING"),
        new CrosswordEntry("A fast, four-legged animal", "CHEETAH"),
        new CrosswordEntry("The opposite of 'left'", "RIGHT"),
        new CrosswordEntry("A large, cold region with ice", "ARCTIC"),
        new CrosswordEntry("A tool used to tighten screws", "SCREWDRIVER"),
        new CrosswordEntry("The smallest unit of life", "CELL"),
        new CrosswordEntry("An animal that hops", "FROG"),
        new CrosswordEntry("A fruit with seeds on the outside", "STRAWBERRY"),
        new CrosswordEntry("The opposite of 'weak'", "STRONG"),


        new CrosswordEntry("A body of water smaller than a sea", "LAKE"),
        new CrosswordEntry("The number of days in a week", "SEVEN"),
        new CrosswordEntry("The season after summer", "AUTUMN"),
        new CrosswordEntry("A tool used to cut paper", "SCISSORS"),
        new CrosswordEntry("The largest land animal", "ELEPHANT"),
        new CrosswordEntry("A flying vehicle used by astronauts", "ROCKET"),
        new CrosswordEntry("The opposite of 'early'", "LATE"),
        new CrosswordEntry("A large reptile with a powerful tail", "CROCODILE"),
        new CrosswordEntry("A person who delivers letters", "MAILMAN"),
        new CrosswordEntry("A fruit with a hard pit and fuzzy skin", "PEACH"),
        new CrosswordEntry("A long, flowing body of water", "RIVER"),
        new CrosswordEntry("The main ingredient in bread", "FLOUR"),

        new CrosswordEntry("The primary material in a pencil", "WOOD"),
        new CrosswordEntry("A flower commonly associated with love", "ROSE"),

        new CrosswordEntry("A vegetable that's often orange", "CARROT"),
        new CrosswordEntry("An object that keeps you dry in the rain", "UMBRELLA"),
        new CrosswordEntry("The planet known as the 'Red Planet'", "MARS"),
        new CrosswordEntry("A sweet treat often eaten on birthdays", "CAKE"),
        new CrosswordEntry("The process of buying and selling goods", "TRADE"),
        new CrosswordEntry("The largest mammal on Earth", "WHALE"),
        new CrosswordEntry("A famous clock tower in London", "BIGBEN"),
        new CrosswordEntry("The opposite of 'soft'", "HARD"),
        new CrosswordEntry("The color of a ripe banana", "YELLOW"),
        new CrosswordEntry("A famous wizarding school in the UK", "HOGWARTS"),
        new CrosswordEntry("The season after winter", "SPRING"),
        new CrosswordEntry("An instrument with six strings", "GUITAR"),
        new CrosswordEntry("A person who flies planes", "PILOT"),
        new CrosswordEntry("A soft drink often served with ice", "COLA"),
        new CrosswordEntry("A tool used to write or draw", "PENCIL"),
        new CrosswordEntry("An animal that lives in a shell", "SNAIL"),
        new CrosswordEntry("The capital of the United States", "WASHINGTON"),
        new CrosswordEntry("A sport played with a racket and ball", "TENNIS"),
        new CrosswordEntry("The season with the longest days", "SUMMER"),
        new CrosswordEntry("A fruit known for its seeds and red flesh", "WATERMELON"),

        new CrosswordEntry("An item worn on the wrist to tell time", "WATCH"),
        new CrosswordEntry("A machine used for baking", "OVEN"),
        new CrosswordEntry("The opposite of 'day'", "NIGHT"),
        new CrosswordEntry("A sport played on ice with a stick and puck", "HOCKEY"),
        new CrosswordEntry("The study of celestial bodies", "ASTRONOMY"),
        new CrosswordEntry("An animal that swings from trees", "MONKEY"),
        new CrosswordEntry("The month with Halloween", "OCTOBER"),
        new CrosswordEntry("A flat surface for eating or working", "TABLE"),
        new CrosswordEntry("A popular holiday plant with red leaves", "POINSETTIA"),
        new CrosswordEntry("The sound made by a cat", "MEOW"),
        new CrosswordEntry("The largest continent on Earth", "ASIA"),
        new CrosswordEntry("A sport played with a hoop and ball", "BASKETBALL"),
        new CrosswordEntry("A tool used to water plants", "HOSE"),
        new CrosswordEntry("A person who catches fish", "FISHERMAN"),
        new CrosswordEntry("The opposite of 'north'", "SOUTH"),
        new CrosswordEntry("The number of sides on a square", "FOUR"),
        new CrosswordEntry("A winter sport with sleds", "BOBSLEDDING"),
       
        new CrosswordEntry("An animal that produces wool", "SHEEP"),
        new CrosswordEntry("The capital city of Italy", "ROME"),
        new CrosswordEntry("A game played with pins and a ball", "BOWLING"),
        new CrosswordEntry("A small bird known for its song", "ROBIN"),
        new CrosswordEntry("A structure built over water", "BRIDGE"),
        new CrosswordEntry("A part of the body used for breathing", "LUNG"),

        new CrosswordEntry("A yellow metal used in jewelry", "GOLD"),
        new CrosswordEntry("A famous bear who loves honey", "POOH"),


        new CrosswordEntry("The place where the American president lives", "WHITEHOUSE"),
        new CrosswordEntry("A popular pasta shape", "SPAGHETTI"),
        new CrosswordEntry("A famous tower in Paris", "EIFFEL"),
        new CrosswordEntry("A popular Italian dish", "PIZZA"),
        new CrosswordEntry("A professional dancer", "BALLERINA"),

        new CrosswordEntry("A tool used to dig", "SHOVEL"),
        new CrosswordEntry("A famous detective", "SHERLOCK"),
        new CrosswordEntry("The capital of the United Kingdom", "LONDON"),
        
        new CrosswordEntry("A popular sport with a ball", "FOOTBALL"),
        new CrosswordEntry("A bird that can mimic sounds", "PARROT"),
        new CrosswordEntry("A precious gemstone", "EMERALD"),
        new CrosswordEntry("A type of cloud", "CUMULUS"),
        new CrosswordEntry("A famous Italian artist", "LEONARDO"),
        new CrosswordEntry("A device used for measuring temperature", "THERMOMETER"),

        new CrosswordEntry("A well-known American scientist", "EINSTEIN"),
        new CrosswordEntry("A plant with thorns", "ROSEBUSH"),
        new CrosswordEntry("A capital city in Japan", "TOKYO"),
        new CrosswordEntry("A place where people go to watch movies", "CINEMA"),
        new CrosswordEntry("A popular music festival", "COACHELLA"),

        new CrosswordEntry("A type of fish found in the ocean", "SALMON"),
        new CrosswordEntry("A large bird that cannot fly", "OSTRICH"),
        new CrosswordEntry("A popular video game character", "MARIO"),

        new CrosswordEntry("A famous superhero", "SPIDERMAN"),

        new CrosswordEntry("A popular beverage made with tea", "ICEDTEA"),
        new CrosswordEntry("A classic fairy tale character", "CINDERELLA"),
        new CrosswordEntry("A well-known fast food restaurant", "MCDONALDS"),
        new CrosswordEntry("A popular software used for design", "PHOTOSHOP"),

        new CrosswordEntry("A famous American president", "LINCOLN"),
        new CrosswordEntry("A historical building in Athens", "PARTHENON"),
        new CrosswordEntry("A type of seafood", "LOBSTER"),

        new CrosswordEntry("A popular beach destination", "HAWAII"),
        new CrosswordEntry("A popular type of pasta", "FETTUCCINE"),
        new CrosswordEntry("A well-known global brand", "NIKE"),

        new CrosswordEntry("A popular social media platform", "INSTAGRAM"),
        new CrosswordEntry("A type of pastry", "CROISSANT"),
        new CrosswordEntry("A famous river in Egypt", "NILE"),
        new CrosswordEntry("A common type of weather", "RAIN"),
        new CrosswordEntry("A famous theme park in the USA", "DISNEYLAND"),
        new CrosswordEntry("A popular style of pizza", "MARGHERITA"),
        new CrosswordEntry("A type of bread made from wheat", "WHOLEWHEAT"),
        new CrosswordEntry("A capital city in Africa", "CAIRO"),
        new CrosswordEntry("A type of outdoor activity", "CAMPING"),
        new CrosswordEntry("A popular video streaming service", "NETFLIX"),
        new CrosswordEntry("A popular outdoor concert", "FESTIVAL"),
        new CrosswordEntry("A type of tree that produces nuts", "CHESTNUT"),
        new CrosswordEntry("A popular beach activity", "SURFING"),

        new CrosswordEntry("A type of weather phenomenon", "THUNDERSTORM"),
        new CrosswordEntry("A popular hiking destination", "MOUNTAIN"),
        new CrosswordEntry("A type of bird known for its long neck", "FLAMINGO"),
        new CrosswordEntry("A famous ancient wonder", "PYRAMID"),
        new CrosswordEntry("A type of tree that produces apples", "APPLETREE"),
        new CrosswordEntry("A place where you buy food", "STORE"),
        new CrosswordEntry("A famous city known for its canals", "VENICE"),

        new CrosswordEntry("A type of cheese", "MOZZARELLA"),
        new CrosswordEntry("A popular American holiday", "THANKSGIVING"),
        new CrosswordEntry("A popular music genre", "ROCK"),
        new CrosswordEntry("A large building for watching performances", "THEATRE"),
        new CrosswordEntry("A type of tree that produces oranges", "CITRUS"),
        new CrosswordEntry("A popular vacation destination", "CARIBBEAN"),
        new CrosswordEntry("A place to learn new skills", "WORKSHOP"),
        new CrosswordEntry("A famous inventor", "EDISON"),
        new CrosswordEntry("A type of fish used in sushi", "TUNA"),
   
        new CrosswordEntry("A popular vegetable often used in salads", "LETTUCE"),

        new CrosswordEntry("A popular social network", "FACEBOOK"),
        new CrosswordEntry("A large, round fruit", "PUMPKIN"),
        new CrosswordEntry("A country known for its ancient ruins", "GREECE"),
        new CrosswordEntry("A place where people go to swim", "POOL"),
        new CrosswordEntry("A popular musical instrument", "PIANO"),
        new CrosswordEntry("A famous river in France", "SEINE"),
        new CrosswordEntry("A famous designer brand", "GUCCI"),
        new CrosswordEntry("A popular type of ice cream", "VANILLA"),
        new CrosswordEntry("A city known for its Eiffel Tower", "PARIS"),
        new CrosswordEntry("A type of tree with needles", "PINE"),
        new CrosswordEntry("A movable barrier used to close an opening", "DOOR"),
        new CrosswordEntry("To reverse an action or command", "UNDO"),
        new CrosswordEntry("The core, central, or most essential part of something", "KERNEL"),
        new CrosswordEntry("Barriers used to prevent flooding", "DYKES"),
        new CrosswordEntry("The criminal act of deliberately setting fire to property", "ARSON"),
        new CrosswordEntry("A list of tasks or things to be completed", "TODO"),
        new CrosswordEntry("A large, cold region covered in ice", "ARTIC"),
        new CrosswordEntry("A popular Italian dessert made of layered coffee-soaked cake", "TIRAMISU"),
        new CrosswordEntry("A large, round fruit with a thick rind", "PINEAPPLE"),
        new CrosswordEntry("A type of bean used in chocolate", "COCOA"),
        new CrosswordEntry("A place where you store books and magazines", "BOOKSHELF"),
        new CrosswordEntry("The first human to walk on the moon", "ARMSTRONG"),
        new CrosswordEntry("The largest desert in the world", "SAHARA"),
        new CrosswordEntry("A country known for its pyramids and pharaohs", "EGYPT"),
        new CrosswordEntry("A body of water that separates two land masses", "STRAIT"),
        new CrosswordEntry("A popular type of pasta shaped like tubes", "PENNE"),
        new CrosswordEntry("A famous mountain in Japan", "FUJI"),
        new CrosswordEntry("A type of dog that is often used in herding", "COLLIE"),
        new CrosswordEntry("A famous writer known for '1984'", "ORWELL"),
        new CrosswordEntry("A small, sweet fruit often used in pies", "CHERRY"),
        new CrosswordEntry("A mythical creature with the body of a lion and the head of an eagle", "GRIFFIN"),
        new CrosswordEntry("A popular type of cheese from France", "BRIE"),
        new CrosswordEntry("A famous ancient city in Greece", "ATHENS"),
        new CrosswordEntry("A red gemstone often used in jewelry", "RUBY"),
        new CrosswordEntry("A rare, precious metal", "PLATINUM"),
        new CrosswordEntry("A tool used to tie things together", "STRING"),

        new CrosswordEntry("A popular vacation destination in the Caribbean", "BAHAMAS"),
        new CrosswordEntry("A type of art often painted on canvas", "PAINTING"),

        new CrosswordEntry("A country famous for kangaroos", "AUSTRALIA"),
        new CrosswordEntry("A bird known for its beautiful song", "NIGHTINGALE"),
        new CrosswordEntry("A large, carnivorous animal found in Africa", "LION"),
        new CrosswordEntry("A popular clothing item for cold weather", "JACKET"),
        new CrosswordEntry("A fruit that is also the name of a color", "ORANGE"),
        new CrosswordEntry("A famous art museum in Paris", "LOUVRE"),
        new CrosswordEntry("A famous American musician known for his guitar skills", "HENDRIX"),
        new CrosswordEntry("A traditional food from Japan", "SUSHI"),
        new CrosswordEntry("A large mountain range in Asia", "HIMALAYAS"),
        new CrosswordEntry("A place where you can see animals in their natural environment", "SAFARI"),
        new CrosswordEntry("A famous landmark in Egypt", "GIZA"),

        new CrosswordEntry("A type of car made by a German manufacturer", "MERCEDES"),

        new CrosswordEntry("A machine that helps clean your floor", "VACUUM"),
        new CrosswordEntry("A famous Canadian singer", "CELINEDION"),
        new CrosswordEntry("A tall, leafy plant often seen in tropical climates", "PALM"),
        new CrosswordEntry("A famous American city known for its skyscrapers", "NEWYORK"),
        new CrosswordEntry("A musical instrument that is often played in orchestras", "VIOLIN"),
        new CrosswordEntry("A famous ancient civilization", "MAYAN"),
        new CrosswordEntry("A type of bird known for its red feathers", "CARDINAL"),
      
        new CrosswordEntry("A famous race car driver", "SCHUMACHER"),
        new CrosswordEntry("A famous national park in the United States", "YELLOWSTONE"),
        new CrosswordEntry("A large, cold region with ice", "POLAR"),
        new CrosswordEntry("A famous American actress", "MERYLSTREEP"),
        new CrosswordEntry("A famous island in the Caribbean", "CUBA"),
        new CrosswordEntry("A country famous for sushi", "JAPAN"),
        new CrosswordEntry("A popular ice cream topping", "CHOCOLATESYRUP"),
        new CrosswordEntry("A famous Broadway musical", "CATS"),
        new CrosswordEntry("A famous TV series", "FRIENDS"),
        new CrosswordEntry("A type of horse known for its speed", "THOROUGHBRED"),
        new CrosswordEntry("A famous painter from Spain", "PICASSO"),
        new CrosswordEntry("A bird known for its long migration", "SWALLOW"),
        new CrosswordEntry("A large animal known for its tusks", "MAMMOTH"),
        new CrosswordEntry("A vegetable often used in salads", "CUCUMBER"),
        new CrosswordEntry("A popular beverage made with lemons", "LEMONADE"),
        new CrosswordEntry("A tool used to measure weight", "SCALES"),
        new CrosswordEntry("A famous natural disaster", "EARTHQUAKE"),
        new CrosswordEntry("A large ocean animal known for its intelligence", "DOLPHIN"),
        new CrosswordEntry("A small, flying insect", "MOTH"),
        new CrosswordEntry("A traditional dance from Spain", "FLAMENCO"),
        new CrosswordEntry("A popular video game character", "SONIC"),
        new CrosswordEntry("A famous mythological creature from Greek mythology", "MINOTAUR"),
        new CrosswordEntry("A famous historical figure from ancient Greece", "SOCRATES"),
        new CrosswordEntry("A bird known for its beautiful feathers", "PEACOCK"),
        
        new CrosswordEntry("A popular fruit used in pies", "APPLE"),
        new CrosswordEntry("A metal that conducts electricity", "COPPER"),
        new CrosswordEntry("Short for an evening event", "GALA"),

        new CrosswordEntry("A basic light source", "LAMP"),
        new CrosswordEntry("A quick way to enter water", "DIVE"),
        new CrosswordEntry("A type of flooring material", "TILE"),
        new CrosswordEntry("The number after two", "THREE"),
        new CrosswordEntry("To seize quickly", "GRAB"),
        new CrosswordEntry("A formal military attack", "RAID"),
        new CrosswordEntry("A warm covering for feet", "SOCK"),
        //  3 LETTER START
        new CrosswordEntry("A vehicle used to carry passengers", "BUS"),
        new CrosswordEntry("A popular British drink", "TEA"),
        new CrosswordEntry("A type of bird that can't fly", "EMU"),
        new CrosswordEntry("A common type of meat", "HAM"),
        new CrosswordEntry("An exclamation of surprise", "WOW"),
        new CrosswordEntry("A sound made by a cow", "MOO"),
        new CrosswordEntry("A sticky substance from trees", "SAP"),
        new CrosswordEntry("An informal word for a male sibling", "BRO"),
        new CrosswordEntry("A small rodent", "RAT"),
        new CrosswordEntry("A common pet that purrs", "CAT"),
        new CrosswordEntry("A tool used for cutting", "AXE"),
        new CrosswordEntry("The start of a musical scale", "DOH"),
        new CrosswordEntry("A short sleep", "NAP"),
        new CrosswordEntry("A small, flying insect known for its sting", "BEE"),
        new CrosswordEntry("The opposite of dry", "WET"),
    };
}


[Serializable]
public class CrosswordStructure
{
    public int crosswordNumber;
    public List<CrosswordEntryPositional> horizontalEntries = new List<CrosswordEntryPositional>();
    public List<CrosswordEntryPositional> verticalEntries = new List<CrosswordEntryPositional>();
}


[Serializable]
public class CrosswordEntryPositional
{
    public int StartX, StartY;
    public bool isHorizontal;
    public CrosswordEntry entry;
     public bool IsEntryFilled;
}


[Serializable]
public class CrosswordEntry
{
    public string question;
    public string answer;


    public CrosswordEntry(string question, string answer)
    {
        this.question = question;
        this.answer = answer;
    }
}