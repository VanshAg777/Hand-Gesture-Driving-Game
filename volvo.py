import cv2
from cvzone.HandTrackingModule import HandDetector

# Parameters
width, height = 1280, 720
import socket
# Webcam
cap = cv2.VideoCapture(0)
cap.set(3, width)
cap.set(4, height)

# Hand Detector
detector = HandDetector(maxHands = 1, detectionCon = 0.8)

# Communication - Using UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5053)

while True:
    # Get the frame from the webcam
    success, img = cap.read()
    # Hands
    hands, img = detector.findHands(img)

    data = []
    # 21 Landmarks. Each landmark has 3 values - x,y and z. (x,y,z) * 21
    if hands:
        # Get the first hand detected
        hand = hands[0]
        # Get the landmark list. It's a dictionary
        lmList = hand['lmList']
        # print(lmList)
        for lm in lmList:
            # We want the data in one line
            data.extend([lm[0], height - lm[1], lm[2]])
        # print(data)
        sock.sendto(str.encode(str(data)), serverAddressPort)

    img = cv2.resize(img, (0, 0), None, 0.5, 0.5)
    cv2.imshow("Image", img)
    cv2.waitKey(1)
