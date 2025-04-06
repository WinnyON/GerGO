from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt


class MultiSelectDropdown(QPushButton):
    def __init__(self):
        super().__init__("Select Columns")
        self.list_widget = QListWidget()
        self.list_widget.setSelectionMode(QListWidget.NoSelection)

        self.menu = QMenu()
        self.menu.setStyleSheet("QMenu { menu-scrollable: 1; }")


        # Create widget action
        widget_action = QWidgetAction(self)
        widget_action.setDefaultWidget(self.list_widget)
        self.menu.addAction(widget_action)
        self.setMenu(self.menu)

    def set_list_items(self, items):
        self.list_widget.clear()
        for item in items:
            self.list_widget.addItem(item)
        # Make items checkable
        for i in range(self.list_widget.count()):
            self.list_widget.item(i).setCheckState(Qt.Unchecked)

    def selected_items(self):
        return [self.list_widget.item(i).text()
                for i in range(self.list_widget.count())
                if self.list_widget.item(i).checkState() == Qt.Checked]


class CreateIndex(QWidget):
    def __init__(self):
        super().__init__()
        self.layout = QGridLayout()
        self.layout.setContentsMargins(20, 30, 20, 30)
        self.setLayout(self.layout)

        self.index_name_label = QLabel("Index name")
        self.index_name_input = QLineEdit()

        self.columns_label = QLabel("Columns")
        self.columns_input = MultiSelectDropdown()

        self.add_widgets()


    def set_column_data(self, columns):
        self.columns_input.set_list_items(columns)

    def add_widgets(self):
        self.layout.addWidget(self.index_name_label, 0, 0)
        self.layout.addWidget(self.index_name_input, 1, 0)
        self.layout.addWidget(self.columns_label, 2, 0)
        self.layout.addWidget(self.columns_input, 3, 0)

    def get_data(self):
        name = self.index_name_input.text()
        columns = self.columns_input.selected_items()
        return name, columns


