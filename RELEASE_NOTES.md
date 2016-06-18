### 3.0.0
* No longer ILMerge dependencies
* BREAKING: Total rewrite of the metrics structure to make it easy to control and customize

### 2.1.0
* Added endpoint (/packages) which lists the nuget packages used at compile time for this application using the content of the packages.config file, if it is found in the application directory.

### 2.0.0
* Add version information to response from API
	* This is done to allow extending the response in future releases
* ILMerge dependencies into Okanshi assembly

### 1.0.1
* Fix bug where CSharp.Monitor would not be usable in conjuction with the API

### 1.0.0
* Intial project release
