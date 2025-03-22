from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font


class MainEditorPage(QWidget):
    def __init__(self):
        super().__init__()
        self.layout = QHBoxLayout()
        self.sidebar_layout = QVBoxLayout()
        self.editor_layout = QStackedLayout()
        self.editor_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.setLayout(self.layout)

        self.db_tree = QTreeWidget()
        self.db_tree.setHeaderHidden(False)
        self.db_tree.setHeaderLabels(['Databases'])
        self.db_tree.header().setFont(h2_font)
        self.db_tree.setStyleSheet("""
			QTreeWidget::item:selected {
			                background-color: #87CEEB;
			                color: #FFFFFF;
			            }
			QTreeWidget {
				margin: 5px;
			}
			QTreeWidget::item {
				margin-right: 40px;
			}
        	""")
        self.nothing_to_see_label = QLabel("Nothing to see yet...")
        self.nothing_to_see_label.setFont(h2_font)
        self.nothing_to_see_label.setAlignment(Qt.AlignmentFlag.AlignCenter)

        self.sidebar_layout.addWidget(self.db_tree)

        self.create_demo_tree()

        self.editor_layout.addWidget(self.nothing_to_see_label)

        self.layout.addLayout(self.sidebar_layout, 1)
        self.layout.addLayout(self.editor_layout, 3)


    def create_demo_tree(self):
    	rootItem1 = QTreeWidgetItem(['Root Item 1'])
    	rootItem2 = QTreeWidgetItem(['Root Item 2'])

    	self.db_tree.addTopLevelItem(rootItem1)
    	self.db_tree.addTopLevelItem(rootItem2)

    	childItem1 = QTreeWidgetItem(['Child Item 1'])
    	rootItem1.addChild(childItem1)

    	self.db_tree.setCurrentItem(childItem1)

    def show_unselected_state(self):
    	self.editor_layout.setCurrentIndex(0)
