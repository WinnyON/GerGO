import sys
from PyQt5.QtWidgets import QApplication, QMainWindow, QWidget, QStackedLayout, QPushButton, QVBoxLayout
from connectPage import ConnectPage
from mainEditorPage import MainEditorPage
from menuBar import MenuBar
import os
import sys

parent_folder = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(1, parent_folder)

from client import Client
from repository import Repository

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.client = Client()
        self.repository = Repository(self.client)
        self.stack_layout = QStackedLayout()
        self.connect_page = ConnectPage(self.stack_layout, self.client)
        self.main_editor_page = MainEditorPage(self.repository)
        self.stack_layout.addWidget(self.connect_page)
        self.stack_layout.addWidget(self.main_editor_page)
        self.container = QWidget()
        self.setWindowTitle("GerGO")
        self.setGeometry(100, 100, 1280, 720)
        self.container.setLayout(self.stack_layout)
        self.setCentralWidget(self.container)


    def switch_to_main_editor_page(self):
        self.stack_layout.setCurrentIndex(1)

if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec_())
