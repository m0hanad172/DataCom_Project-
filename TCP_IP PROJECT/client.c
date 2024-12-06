#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <winsock2.h>

#pragma comment(lib, "ws2_32.lib")

void playGame(SOCKET socket);

int main() {
    WSADATA wsa;
    SOCKET clientSocket;
    struct sockaddr_in server;
    char serverResponse[1024];

    printf("Initializing Winsock...\n");
    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) {
        printf("Failed. Error Code : %d\n", WSAGetLastError());
        return 1;
    }

    if ((clientSocket = socket(AF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET) {
        printf("Could not create socket: %d\n", WSAGetLastError());
        return 1;
    }

    server.sin_addr.s_addr = inet_addr("127.0.0.1");
    server.sin_family = AF_INET;
    server.sin_port = htons(5000);

    if (connect(clientSocket, (struct sockaddr*)&server, sizeof(server)) < 0) {
        printf("Connection failed. Error: %d\n", WSAGetLastError());
        return 1;
    }

    printf("Connected to the server!\n");
    playGame(clientSocket);

    closesocket(clientSocket);
    WSACleanup();
    return 0;
}

void playGame(SOCKET socket) {
    char message[1024], serverReply[1024];

    while (1) {
        printf("Choose your move: 1 (Rock), 2 (Paper), 3 (Scissors), or 'exit' to quit: ");
        fgets(message, sizeof(message), stdin);
        message[strcspn(message, "\n")] = 0; // Remove trailing newline

        if (send(socket, message, strlen(message), 0) < 0) {
            printf("Send failed.\n");
            break;
        }

        int recv_size = recv(socket, serverReply, sizeof(serverReply), 0);
        if (recv_size == SOCKET_ERROR) {
            printf("Receive failed.\n");
            break;
        }

        serverReply[recv_size] = '\0';
        printf("%s\n", serverReply);

        if (strcmp(message, "exit") == 0) {
            printf("Game ended. Final scores displayed above.\n");
            break;
        }
    }
}