from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h1_font

class ConnectPage(QWidget):
    def __init__(self):
        super().__init__()
        self.layout = QGridLayout()
        self.form_layout = QFormLayout()
        self.welcome_layout = QHBoxLayout()
        self.submit_layout = QHBoxLayout()       
        self.connection_failed_layout = QHBoxLayout()
        self.layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.welcome_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.submit_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.connection_failed_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.setLayout(self.layout)

        self.welcome_message = QLabel("WELCOME TO GERGO")
        self.welcome_message.setFont(h1_font)
        self.welcome_message.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.ip_label = QLabel("IP")
        self.port_label = QLabel("Port")
        self.input_ip = QLineEdit()
        self.input_port = QLineEdit()
        self.submit_button = QPushButton("Connect to server")
        self.connection_failed_label = QLabel("Could not connect to the server. Try again!")
        self.connection_failed_label.setAlignment(Qt.AlignmentFlag.AlignCenter)

        self.welcome_layout.addWidget(self.welcome_message)
        self.layout.addLayout(self.welcome_layout, 0, 0, 1, 2)

        self.form_layout.addRow(self.ip_label, self.input_ip)
        self.form_layout.addRow(self.port_label, self.input_port)
        self.form_layout.setContentsMargins(20, 20, 20, 20)
        self.layout.addLayout(self.form_layout, 1, 0, 2, 2)
        # self.layout.addWidget(self.ip_label)
        # self.layout.addWidget(self.input_ip)
        # self.layout.addWidget(self.port_label)
        # self.layout.addWidget(self.input_port)
        self.submit_layout.addWidget(self.submit_button)
        self.submit_layout.setContentsMargins(100, 0, 100, 0)
        self.layout.addLayout(self.submit_layout, 3, 0, 1, 2)


        self.connection_failed_layout.addWidget(self.connection_failed_label)
        self.layout.addLayout(self.connection_failed_layout, 4, 0, 1, 2)
        self.connection_failed_label.hide()

    def failed_connection():
        self.connection_failed_label.show()