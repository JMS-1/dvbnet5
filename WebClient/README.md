# VCR.NET Recording Service Web Application

The web application uses the VCR.NET HTTP/REST API to make all functionality of the recording service available to the user. For the implementation the React framework is used.

The architecture is based on an experimental concept I name **NoUI**. Each view element (TSX) should only implement interacting with the user - and in most cases this is the case. A related NoUI instance provides all necessary data and callbacks - in fact identical to what a view model will do. Planned but never realized is a complete testing based on the NoUI layer. This concept of mine is very old and modern frameworks linke Angular work in a very similiar way - if the concepts of the framework are used consequently as suggested.
