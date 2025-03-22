import sys
from PyQt5.QtWidgets import QApplication, QMainWindow, QWidget, QStackedLayout, QPushButton, QVBoxLayout
from connectPage import ConnectPage

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.stack_layout = QStackedLayout()
        self.connect_page = ConnectPage()
        self.stack_layout.addWidget(self.connect_page)
        self.container = QWidget()
        self.setWindowTitle("GerGO")
        self.setGeometry(100, 100, 1280, 720)
        self.container.setLayout(self.stack_layout)
        self.setCentralWidget(self.container)

    # def switch_to_second_page(self):
    #     self.stack_layout.setCurrentIndex(1)

if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec_())
