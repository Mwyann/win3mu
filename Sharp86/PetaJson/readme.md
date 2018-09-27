# PetaJson

PetaJson is a simple but flexible JSON library implemented in a single C# file.  Features include:

* Standard JSON parsing and generation
* Supports strongly typed serialization through reflection or custom code
* Supports weakly typed serialization
* Supports standard C# collection classes - no JSON specific classes (ie: no "JArray", "JObject" etc...)
* Support for dynamic Expando (read) and anonymous types (write)
* Choose from good performance + portable (System.Reflection), or high performance + less portable (using System.Reflection.Emit)
* Custom formatting and parsing of any type
* Support for serialization of polymorphic types
* Utilities for cloning and reparsing objects into different types
* Bonus `IDictionary<string,object>` extensions simplifies working with weakly typed JSON data.
* Directly reads from TextReader and writes to TextWriter and any underlying stream
* Simple set of custom attributes to control serialization
* Optional non-strict parsing allows comments, non-quoted dictionary keys, trailing commas and hex literals (great for config files)
* Optional pretty formatting
* No dependencies, one file - PetaJson.cs
* Works on .NET, Mono, Xamarin.Android, Xamarin.iOS.

# Usage

Here goes, a 5 minute whirl-wind tour of using PetaJson...

## Setup

1. Add PetaJson.cs to your project
2. That's it

Well almost, you'll probably want some `using PetaJson;` clauses and depending on your target platform you might need to define the following symbols in your project to  disable some functionality:

* `PETAJSON_NO_DYNAMIC` - disable support for System.Dynamic.ExpandoObject
* `PETAJSON_NO_EMIT` - disable use of System.Reflection.Emit (slower, more portable)
* `PETAJSON_NO_DATACONTRACT` - disable support for System.Runtime.Serialization.DataContract/Member

## Generating JSON

To a string:

	var o = new int[] { 1, 2, 3 };
	var json = Json.Format(o);

or, write to a file

	Json.WriteFile("MyData.json", o);

using objects

	class Person
	{
		string Name;
		string Address;
	};

	var p = new Person() { Name = "Joe Sixpack", Address = "Home" };
	var json = Json.Format(p);

would yield:

	{
		"name": "Joe Sixpack",
		"address": "Home"
	}


## Parsing JSON

From a string:

	int o = Json.Parse<int>("23");

From string to a dynamic:

	dynamic o = Json.Parse<object>("{\"apples\":\"red\", \"bananas\":\"yellow\" }");
	string appleColor = o.apples;
	string bananaColor = o.bananas;

Weakly typed dictionary:

	var dict = Json.Parse<Dictionary<string, object>>("{\"apples\":\"red\", \"bananas\":\"yellow\" }");

Or an array:

	int[] array = Json.Parse<int[]>("[1,2,3]");

Strongly typed object:

	Person person = Json.Parse<Person>(jsonFromPersonExampleAbove);
	Console.WriteLine(person.Name);
	Console.WriteLine(person.Address);

From a file:

	var person = Json.ParseFile<Person>("aboutme.json");

String into an existing instance:

	Json.ParseInto<Person>(jsonFromPersonExampleAbove, person);

From file into an existing instance:

	var person = new Person();
	Json.ParseFileInto<Person>("aboutme.json", person);


## Attributes

PetaJson provides two attributes for decorating objects for serialization - [Json] and [JsonExclude].

The [Json] attribute when applied to a class or struct marks all public properties and fields for serialization:

	[Json]
	class Person
	{
		public string Name;				// Serialized as "name"
		public string Address;			// Serialized as "address"
		public string AlsoSerialized;	// Serialized as "alsoSerialized"
		private string NotSerialized;
	}

When applied to one or more field or properties but not applied to the class itself, only the decorated members
will be serialized:

	class Person
	{
		[Json] public string Name;	// Serialized as "name":
		public string Address;		// Not serialized
	}

By default members are serialized using the name of the field or property with the first letter
lowercased.  To override the serialized name, include the name as a parameter to the [Json] attribute:

	class Person
	{
		[Json("PersonName")] public string Name; 	// Serialized as "PersonName"
	}

Use the [JsonExclude] attribute to exclude public fields or properties from serialization

	[Json]
	class Person
	{
		public string Name;		// Serialized as "name"
		public string Address;	// Serialized as "address"

		[JsonExclude]			// Not serialized
		public int Age
		{
			get { return calculateAge(); }
		}
	}

Sometimes you'll want sub-objects to be serialized into an existing object instance.

eg: 

	class MyApp
	{
		public MyApp()
		{
			// Settings object has an owner reference that needs to be maintained
			// across serialization
			CurrentSettings = new Settings(this);
		}

		[Json(KeepInstance=true)]
		Settings CurrentSettings;
	}

In this example the existing CurrentSettings object will be serialized into. If KeepInstance
was set to false, PetaJson would instantiate a new Settings object, load it and then assign
it to the CurrentSettings property.

## DataContract and DataMember attributes

You can also use the system supplied DataContract and DataMember attributes.  They'll only be used if there
are no PetaJson attributes on the class or it's members. You must specify DataContract on the type and
DataMember on all members that require serialization.  

	[DataContract]
	class Person
	{
		[DataMember] public string Name;		// Serialized as "Name"
		[DataMember] public string Address;		// Serialized as "Address"
		[DataMember(Name="Cool")]
		public string Hot;						// Serialized as "Cool"
		public int Age {...}					// Not serialized
	}

Note that the first letter of the member is left as upper case (unlike when using the Json attributes) and
there's no need for an exclude attribute as only members marked DataMember are included in the first place.

## Custom Formatting

Custom formatting can be used on any type.  Say we have the following type:

    struct Point
    {
        public int X;
        public int Y;
    }

and we want to serialize points as a comma separated string like this:

	{
		"TopLeft": "10,20",
		"BottomRight: "30,40",
	}

To do this, we can to register a formatter:

    // Register custom formatter
    Json.RegisterFormatter<Point>( (writer,point) => 
    {
        writer.WriteStringLiteral(string.Format("{0},{1}", point.X, point.Y));
    });

And a custom parser:

    Json.RegisterParser<Point>( literal => {

        var parts = ((string)literal).Split(',');
        if (parts.Length!=2)
            throw new InvalidDataException("Badly formatted point");

        return new Point()
        {
            X = int.Parse(parts[0], CultureInfo.InvariantCulture),
            Y = int.Parse(parts[0], CultureInfo.InvariantCulture),
        };

    });

We can now format and parse Points:

	// Format a Point
	var json = Json.Format(new Point() { X= 10, Y=20 });		// "10,20"

	// Parse a Point
	var point = Json.Parse<Point>("\"10,20\"");

Note that in this example we're formatting to a single string literal.  We can do more
complex custom serialization using the IJsonReader and IJsonWriter interfaces - see below.

## Simple Formatting for Value Types

The problem with the above method is that it requires pre-registering the formatters and 
parsers which can be a pain. Another way to do custom formatting for value types is by 
implementing methods directly on the struct.  This approach is more intrusive but also 
more self-contained.

The methods must have the following method names and signatures and can be public or not.

	// One of these:
	void FormatJson(IJsonWriter w);
	string FormatJson();

	// And one of these
	static T ParseJson(IJsonReader r);
	static T ParseJson(string literal);

For example, this is the equivalent of the above example:

    struct Point
    {
        public int X;
        public int Y;

		string FormatJson() 
		{ 
			return string.Format("{0},{1}", X, Y);
		};

		static Point ParseJson(string literal)
		{
			var parts = literal.Split(',');
			if (parts.Length!=2)
				throw new InvalidDataException("Badly formatted point");

			return new Point()
			{
				X = int.Parse(parts[0], CultureInfo.InvariantCulture),
				Y = int.Parse(parts[0], CultureInfo.InvariantCulture),
			};
		}
    }

Note: this approach only works for structs (not classes)

## Custom Factories and Polymorphic Types

Suppose we have a class heirarchy something like this:

    abstract class Shape
    {
    	// Omitted
    }

    class Rectangle : Shape
    {
    	// Omitted
    }

    class Ellipse : Shape
    {
    	// Omitted
    }

and we'd like to serialize a list of Shapes to JSON like this:

	[
		{ "kind": "Rectangle", /* other rectangle properties omitted */ },
		{ "kind": "Shape", /* other ellipse properties omitted */ },
		// etc...
	]

In other words a value in the JSON dictionary determines the type of object that 
needs to be instantiated for that element.

We can write out the shape kind by implementing the IJsonWriting interface which gets called
before the other properties of the object are written:

    abstract class Shape : IJsonWriting
    {
        // Override OnJsonWriting to write out the derived class type
        void IJsonWriting.OnJsonWriting(IJsonWriter w)
        {
            w.WriteKey("kind");
            w.WriteStringLiteral(GetType().Name);
        }
    }

For parsing, we need to register a callback function that creates the correct instances:

    // Register a type factory that can instantiate Shape objects
    Json.RegisterTypeFactory(typeof(Shape), (reader, key) =>
    {
        // This method will be called back for each key in the json dictionary
        // until an object instance is returned

        // We wrote the object type using the key "kind", look for it
        if (key != "kind")
            return null;

        // Read the next literal and instantiate the correct object type
        return reader.ReadLiteral(literal =>
        {
            switch ((string)literal)
            {
                case "Rectangle": return new Rectangle();
                case "Ellipse": return new Ellipse();
                default:
                    throw new InvalidDataException(string.Format("Unknown shape kind: '{0}'", literal));
            }
        });
    });

When attempting to deserialize Shape objects, the registered callback will be called with each 
key in the dictionary until it returns an object instance.  In this case we're looking for a key
named "kind" and we use it's value to create either a Rectangle or Ellipse.

Note that the field used to hold the type (ie: "kind") does not need to be the first field in the
 in the dictionary being parsed. After instantiating the object, the input stream is re-wound to the
 start of the dictionary and then re-parsed into the instantiated object.  Note too that
 the underlying stream doesn't need to support seeking - the rewind mechanism is implemented in 
 PetaJson.

## Serialization Events

An object can receive notifications of various events during the serialization/deserialization process
by implementing one or more of the following interfaces:

    // Called before loading via reflection
    public interface IJsonLoading
    {
        void OnJsonLoading(IJsonReader r);
    }

    // Called after loading via reflection
    public interface IJsonLoaded
    {
        void OnJsonLoaded(IJsonReader r);
    }

    // Called for each field while loading from reflection
    // Return true if handled
    public interface IJsonLoadField
    {
        bool OnJsonField(IJsonReader r, string key);
    }

    // Called when about to write using reflection
    public interface IJsonWriting
    {
        void OnJsonWriting(IJsonWriter w);
    }

    // Called after written using reflection
    public interface IJsonWritten
    {
        void OnJsonWritten(IJsonWriter w);
    }


For example, it's often necessary to wire up ownership references on loaded sub-objects:

	class Drawing : IJsonLoaded
	{
		[Json]
		public List<Shape> Shapes;

		void IJsonLoaded.OnJsonLoaded()
		{
			// Shapes have been loaded, set owner references
			Shapes.ForEach(x => x.Owner = this);
		}
	}

The IJsonLoadField interface can be used to "fix up" incorrect incoming JSON data.  For example, 
imagine a situation where a numeric ID field was incorrectly provided by a server as a string 
(enclosed in quotes) instead of a plain number.


	class MyRecord : IJsonLoadField
	{
		[Json] long id;				// Note: numeric (not string) field
		[Json] string description;

		// Override OnJsonField to intercept the bad server data
		bool IJsonLoadField.OnJsonField(IJsonReader r, string key)
		{
			// id provided as string? Eg: "id": "1234"
			if (key=="id" && r.GetLiteralKind()==LiteralKind.String)
			{
				// Parse the string
				id = long.Parse(r.GetLiteralString());

				// Skip the string literal now that we've handled it
				r.NextToken();		

				// Return true to suppress default processing 
				return true;
			}

			// Other keys and non-quoted id field values processed as normal
			return false;			
		}
	}

Note: although these event methods could have been implemented using reflection rather than interfaces,
the use of interfaces is more discoverable through Intellisense/Autocomplete.

## Cloning and Re-parsing Objects

PetaJson includes a couple of helper functions for cloning objects by saving to them to JSON and then reloading:

	var person1 = new Person() { Name = "Mr Json Bourne"; }
	var person2 = Json.Clone(person1);

You can also clone into an existing instance

	var person3 = new Person();
	Json.CloneInto(person3, person1);		// Copies from person1 to person3

Similar to cloning is re-parsing. While cloning copies from one object to another of the same type,
reparsing allows converting from one object type to another.  For example you can convert a dictionary
of values into a person:

	IDictionary<string,object> dictionary = getDictionaryFromSomewhere();
	var person = Json.Reparse<Person>(dictionary);

You can also go the other way:

	var dictionary = Json.Reparse<IDictionary<string,object>>(person);

## Bonus Dictionary Helpers

PetaJson includes some super handy extensions to `IDictionary<string,object>` that make working
with weakly typed JSON data easier.  Some of these methods are particularly handy when an app
is using JSON to store configuration options or settings.

Suppose we have the following JSON:

	{
		"settings":
		{
			"userSettings":
			{
				"username":"jsonbourne23",
				"password":"123",
				"email":"json@bourne.com",
			},
			"appSettings":
			{
				"firstRun":false,
				"serverUrl":"http://www.toptensoftware.com",
			}
		}
	}

and we parse all this into a weakly typed dictionary:

	var data = Json.ParseFile<IDictionary<string,object>>("settings.json");

We can get a setting like this:

	bool firstRun = data.GetPath<bool>("settings.appSettings.firstRun", true);

Or set it like this:

	data.SetPath("settings.appSettings.firstRun", false);

SetPath creates the path using a set of Dictionary<string,object> if necessary:

	var data = new Dictionary<String, object>();
	data.SetPath("settings.appSettings.serverUrl", "http://whatever.com");

GetPath can reparse if necessary to satify the requested type:

	var userSettings = data.GetPath<UserSettings>("settings.userSettings", null);

You can check if a path exists like this:

	if (data.PathExists("settings.appSettings"))
	{
		// yep
	}

And finally, there's `T GetObjectAtPath<T>(string path)` which does a few things:

1. Makes sure the path exists, and creates it if it doesn't
2. If the path does exist, reparses whatever it finds there into type T.
3. If the path doesn't exists, creates a new T
4. Stores the T instance back into the dictionary at that path.

So now we can work with parts of a weakly typed JSON dictionary with strong types.

eg: 

	var userSettings = data.GetObjectAtPath<UserSettings>("settings.userSettings");

and saving data, will get the changes:

	// Make a change
	userSettings.email = "newemail@bourne.com";

	// It sticks...
	var json = Json.Format(data);
	System.Diagnostic.Assert(json.IndexOf("newemail")>=0);

Note: GetObjectAtPath only works with reference types, not structs.

## Options

PetaJson has a couple of formatting/parsing options. These can be set as global defaults:

	Json.WriteWhitespaceDefault = true;		// Pretty formatting
	Json.StrictParserDefault = true;		// Enable strict parsing

or, provided on a case by case basis:

	Json.Format(person, JsonOption.DontWriteWhitespace);		// Force pretty formatting off
	Json.Format(person, JsonOption.WriteWhitespace);			// Force pretty formatting on
	Json.Parse<object>(jsonData, JsonOption.StrictParser);		// Force strict parsing
	Json.Parse<object>(jsonData, JsonOption.NonStrictParser);	// Disable strict parsing

Non-strict mode relaxes the parser to allow:

* Inline C /* */ or C++ // style comments
* Trailing commas in arrays and dictionaries
* Non-quoted dictionary keys
* Hex number literals

eg: the non-strict parser will allow this:

	{
		/* This is a C-style comment */
		"quotedKey": "allowed",
		nonQuotedKey: "also allowed",
		"arrayWithTrailingComma": [1,2,3,],	
		"hexAllowed": 0x1234,
		"trailing commas": "allowed ->",	// <- see the comma, not normally allowed
	}

## IJsonReader and IJsonWriter

These interfaces only need to be used when writing custom formatters and parsers.  They are the low
level interfaces used to read and write the JSON stream and are passed to the callbacks for custom
parsers and formatters.

### IJsonReader

The IJsonReader interface reads from the JSON input stream.  

    public interface IJsonReader
    {
        object ReadLiteral(Func<object, object> converter);
        void ReadDictionary(Action<string> callback);
        void ReadArray(Action callback);
        object Parse(Type type);
        T Parse<T>();
        LiteralKind GetLiteralKind();
        string GetLiteralString();
        void NextToken();
    }

*ReadLiteral* - reads a single literal value from the input stream.  Throws an exception if
the next token isn't a literal value.  You should provide a callback that converts the raw
literal to the required value, which will then be returned as the return value from ReadLiteral.

Wherever possible, conversion should be done in the callback to ensure that errors in the conversion
report the error location just before the bad literal, instead of after it.


*ReadDictionary* - reads a JSON dictionary, calling the callback for each key encountered.  The
callback routine should read the key's value using the IJsonReader interface.  If nothing is read
by the callback, PetaJson will skip the value and move onto the next key.

*ReadArray* - reads a JSON array, calling the callback at each element position. The callback 
routine must read each value from the IJsonReader before returning.

*Parse* - parses a typed value from the input stream.

*GetLiteralKind*, *GetLiteralString* and *NextToken* provide ability to read literals without boxing
the value into an Object.  Used by the "Reflection.Emit" type parsers, these are much faster than 
ReadLiteral, but less convenient to use.

### IJsonWriter

The IJsonWriter interface writes to the JSON output stream:

    public interface IJsonWriter
    {
        void WriteStringLiteral(string str);
        void WriteRaw(string str);
        void WriteArray(Action callback);
        void WriteDictionary(Action callback);
        void WriteValue(object value);
        void WriteElement();
        void WriteKey(string key);
    }

*WriteStringLiteral* - writes a string literal to the output stream, including the surrounding quotes and
 escaping the content as required.

*WriteRaw* - writes directly to the output stream.  Use for comments, or self generated JSON data.

*WriteArray* - writes an array to the output stream.  The callback should write each element.

*WriteDictionary* - writes a dictionary to the output stream.  The callback should write each element.

*WriteValue* - formats and writes any object value.

*WriteElement* - call from the callback of WriteArray to indicate that the next element is about to be 
written.  Causes PetaJson to write separating commas and whitespace.

*WriteKey* - call from the callback of WriteDictionary to write the key part of the next element.  Writes
whitespace, separating commas, the key and it's quotes, the colon.

eg: to write a dictionary:

	writer.WriteDictionary(() =>
	{
		writer.WriteKey("apples");
		writer.WriteValue("red");
		writer.WriteKey("bananas");
		writer.WriteValue("yellow");
	});

eg: to write an array:

	writer.WriteArray(()=>
	{
		for (int i=0; i<10; i++)
		{
			writer.WriteElement();
			writer.WriteValue(i);
		}
	});

## Performance

Wondering about performance?  When Reflection.Emit is enabled, PetaJson is right up there with
the best of them.  Some simple benchmarks serializing a long list of objects with a mix of 
different primitive types yielded this: (smaller tick value = quicker, better)

	PetaJson     format: 491865 ticks
	Json.NET     format: 757618 ticks x1.54
	ServiceStack format: 615091 ticks x1.25

	PetaJson      parse: 1011818 ticks
	Json.NET      parse: 1204574 ticks x1.19
	ServiceStack  parse: 1177895 ticks x1.16

Although this test shows PetaJson to be quicker, different data types may yield different results.  In 
otherwords: I tested enough to make sure it wasn't ridiculously slow, but haven't done extensive benchmarks.

## License

Copyright (C) 2014 Topten Software (contact@toptensoftware.com)
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this product except in compliance with the License.
You may obtain a copy of the License at

<http://www.apache.org/licenses/LICENSE-2.0>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
