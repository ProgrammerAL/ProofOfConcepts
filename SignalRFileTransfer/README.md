# SignalR File Transfer
The purpose of this application is to demonstrate sending files from a client to a server using SignalR.

The basic steps are:
1. Client loads the file into memory
1. Client separates the file into individual chunks of bytes
1. Client sends each chunk to the server hub
1. Server Hub keeps track of the individual chunks
1. Once the Server Hub has received all chunks for an individual file, an event is raised and a message displayed on the main page

