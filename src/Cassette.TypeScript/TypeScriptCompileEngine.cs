﻿// SNIPPED FROM BundleTransformer project - commit 4c6946b7979e
// Snipped on 1/30/2013 by @George_Hahn
// Licensed under Apache license

using System.IO;
using System.Reflection;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.ErrorMessageHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MsieJavaScriptEngine;
using MsieJavaScriptEngine.ActiveScript;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BundleTransformer.TypeScript.Compilers
{

    //using Core;
    //using Core.Assets;
    //using Core.ErrorMessageHelpers;
    //using CoreStrings = Core.Resources.Strings;

	/// <summary>
	/// TypeScript-compiler
	/// </summary>
	public class TypeScriptCompileEngine : IDisposable
	{
		/// <summary>
		/// Name of resource, which contains a TypeScript-library
		/// </summary>
		const string TYPESCRIPT_LIBRARY_RESOURCE_NAME = "BundleTransformer.TypeScript.Resources.typescript.min.js";

		/// <summary>
		/// Name of resource, which contains a TypeScript-compiler helper
		/// </summary>
		const string TSC_HELPER_RESOURCE_NAME = "BundleTransformer.TypeScript.Resources.tscHelper.min.js";

		/// <summary>
		/// Name of resource, which contains a default lib.d.ts with global declarations
		/// </summary>
		const string DEFAULT_LIBRARY_RESOURCE_NAME = "BundleTransformer.TypeScript.Resources.lib.d.ts";

		/// <summary>
		/// Template of function call, which is responsible for compilation
		/// </summary>
		const string COMPILATION_FUNCTION_CALL_TEMPLATE = @"typeScriptHelper.compile({0}, {1}, {2});";

		/// <summary>
		/// Flag for whether to include a default lib.d.ts with global declarations
		/// </summary>
		private readonly bool _useDefaultLib;

		/// <summary>
		/// String representation of the TypeScript-compiler default options
		/// </summary>
		private readonly string _defaultOptionsString;

		/// <summary>
		/// Common types definitions (contents of the file lib.d.ts)
		/// </summary>
		private readonly string _commonTypesDefinitions;

		/// <summary>
		/// MSIE JS engine
		/// </summary>
		private MsieJsEngine _jsEngine;

		/// <summary>
		/// Synchronizer of compilation
		/// </summary>
		private readonly object _compilationSynchronizer = new object();

		/// <summary>
		/// Flag that compiler is initialized
		/// </summary>
		private bool _initialized;

		/// <summary>
		/// Flag that object is destroyed
		/// </summary>
		private bool _disposed;


		/// <summary>
		/// Constructs instance of TypeScript-compiler
		/// </summary>
		public TypeScriptCompileEngine()
			: this(true)
		{ }

		/// <summary>
		/// Constructs instance of TypeScript-compiler
		/// </summary>
		/// <param name="useDefaultLib">Flag for whether to include a default lib.d.ts 
		/// with global declarations</param>
		public TypeScriptCompileEngine(bool useDefaultLib)
			: this(useDefaultLib, null)
		{ }

		/// <summary>
		/// Constructs instance of TypeScript-compiler
		/// </summary>
		/// <param name="useDefaultLib">Flag for whether to include a default lib.d.ts 
		/// with global declarations</param>
		/// <param name="defaultOptions">TypeScript-compiler default options</param>
		public TypeScriptCompileEngine(bool useDefaultLib, object defaultOptions)
		{
			_useDefaultLib = useDefaultLib;
			_defaultOptionsString = JsonConvert.SerializeObject(defaultOptions);
			if (_useDefaultLib)
			{
				_commonTypesDefinitions = GetResourceAsString(
					DEFAULT_LIBRARY_RESOURCE_NAME, GetType());
			}
		}

        /// <summary>
        /// Gets a content of the embedded resource as string
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="type">Type from assembly that containing an embedded resource</param>
        /// <returns>Сontent of the embedded resource as string</returns>
        public static string GetResourceAsString(string resourceName, Type type)
        {
            Assembly assembly = type.Assembly;

            return GetResourceAsString(resourceName, assembly);
        }

        /// <summary>
        /// Gets a content of the embedded resource as string
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="assembly">Assembly that containing an embedded resource</param>
        /// <returns>Сontent of the embedded resource as string</returns>
        public static string GetResourceAsString(string resourceName, Assembly assembly)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new NullReferenceException(
                        string.Format("Resource with name '{0}' is null.", resourceName));
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

		/// <summary>
		/// Destructs instance of TypeScript-compiler
		/// </summary>
		~TypeScriptCompileEngine()
		{
			Dispose(false /* disposing */);
		}


		/// <summary>
		/// Initializes compiler
		/// </summary>
		private void Initialize()
		{
			if (!_initialized)
			{
				Type type = GetType();

				_jsEngine = new MsieJsEngine(true, false);
				_jsEngine.ExecuteResource(TYPESCRIPT_LIBRARY_RESOURCE_NAME, type);
				_jsEngine.ExecuteResource(TSC_HELPER_RESOURCE_NAME, type);

				_initialized = true;
			}
		}

		/// <summary>
		/// "Compiles" TypeScript-code to JS-code
		/// </summary>
		/// <param name="content">Text content written on TypeScript</param>
		/// <param name="dependencies">List of dependencies</param>
		/// <param name="options">TypeScript-compiler options</param>
		/// <returns>Translated TypeScript-code</returns>
		public string Compile(string content, IList<Dependency> dependencies, object options = null)
		{
			string newContent;
			string optionsString;
			if (options != null)
			{
				optionsString = JsonConvert.SerializeObject(options);
			}
			else
			{
				optionsString = _defaultOptionsString;
			}

			var newDependencies = new List<Dependency>();
			if (_useDefaultLib)
			{
				var defaultLibDependency = new Dependency
				{
				    Url = "lib.d.ts", 
					Path = "lib.d.ts", 
					Content = _commonTypesDefinitions
				};
				newDependencies.Add(defaultLibDependency);
			}
			newDependencies.AddRange(dependencies);

			lock (_compilationSynchronizer)
			{
				Initialize();

				try
				{
					var result = _jsEngine.Evaluate<string>(
						string.Format(COMPILATION_FUNCTION_CALL_TEMPLATE,
							JsonConvert.SerializeObject(content),
							JsonConvert.SerializeObject(newDependencies.Select(d => 
								new { url = d.Url, path = d.Path, content = d.Content })), 
							optionsString));
					var json = JObject.Parse(result);

					var errors = json["errors"] != null ? json["errors"] as JArray : null;
					if (errors != null && errors.Count > 0)
					{
						throw new TypeScriptCompilingException(FormatErrorDetails(errors[0], content));
					}

					newContent = json.Value<string>("compiledCode");
				}
				catch (ActiveScriptException e)
				{
					throw new TypeScriptCompilingException(
						ActiveScriptErrorFormatter.Format(e));
				}
			}

			return newContent;
		}

		/// <summary>
		/// Generates a detailed error message
		/// </summary>
		/// <param name="errorDetails">Error details</param>
		/// <param name="sourceCode">Source code</param>
		/// <returns>Detailed error message</returns>
		private static string FormatErrorDetails(JToken errorDetails, string sourceCode)
		{
			var startIndex = errorDetails.Value<int>("startIndex");
			var message = errorDetails.Value<string>("message");
			var errorSourceInfo = ErrorSourceInfo.Create(sourceCode, startIndex);

			var errorMessage = new StringBuilder();
			errorMessage.AppendLine(string.Format("{0}: {1}", "Message", message));
			errorMessage.AppendLine(string.Format("{0}: {1}", "Line number",
				errorSourceInfo.LineNumber.ToString()));
			errorMessage.AppendLine(string.Format("{0}: {1}", "Column number",
				errorSourceInfo.ColumnNumber.ToString()));
			errorMessage.AppendLine(string.Format("{0}: {2}{1}", "Source error",
				errorSourceInfo.SourceError, Environment.NewLine));

			return errorMessage.ToString();
		}

		/// <summary>
		/// Destroys object
		/// </summary>
		public void Dispose()
		{
			Dispose(true /* disposing */);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Destroys object
		/// </summary>
		/// <param name="disposing">Flag, allowing destruction of 
		/// managed objects contained in fields of class</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;

				if (_jsEngine != null)
				{
					_jsEngine.Dispose();
				}
			}
		}
	}
}
