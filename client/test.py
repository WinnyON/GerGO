from socket import *

def server():
    serverName = 'localhost'
    serverPort = 12000
    serverSocket = socket(AF_INET, SOCK_STREAM)
    serverSocket.setsockopt(SOL_SOCKET, SO_REUSEADDR, 1)
    serverSocket.bind(('', serverPort))
    serverSocket.listen()
    print('The server is ready to receive')
    running = True
    while running:
        connectionSocket, addr = serverSocket.accept()
        sentence = connectionSocket.recv(1024).decode()

        if sentence == "exit":
            running = False

        capitalizedSentence = sentence.upper()
        connectionSocket.send(capitalizedSentence.encode())
        connectionSocket.close()
    serverSocket.close()
    print('The server stopped')

server()