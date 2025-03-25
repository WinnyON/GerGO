from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font
from editorFrame import EditorFrame
from createPage import CreatePage
from menuBar import MenuBar
from dbTree import DbTree


class MainEditorPage(QWidget):
    def __init__(self, repository):
        super().__init__()
        self.repository = repository
        # self.selected_db = None 
        # self.selected_table = None
        self.extended_layout = QVBoxLayout()
        self.layout = QHBoxLayout()
        self.sidebar_layout = QVBoxLayout()
        self.editor_layout = QStackedLayout()
        self.editor_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        # self.setLayout(self.layout)
        self.setLayout(self.extended_layout)

        self.editor_frame = EditorFrame(repository)
        self.db_tree = DbTree(self, self.editor_frame)
        self.create_page = CreatePage(self.editor_layout, self.repository, self.db_tree)
        # self.menu_bar = MenuBar(self)

        # self.extended_layout.addWidget(self.menu_bar)
        self.extended_layout.addLayout(self.layout)

        
        # self.db_tree = QTreeWidget()
        # self.db_tree.setHeaderHidden(False)
        # self.db_tree.setHeaderLabels(['Databases'])
        # self.db_tree.header().setFont(h2_font)
        # self.db_tree.setStyleSheet("""
		# 	QTreeWidget::item:selected {
		# 	                background-color: #87CEEB;
		# 	                color: #FFFFFF;
		# 	            }
		# 	QTreeWidget {
		# 		margin: 5px;
		# 	}
		# 	QTreeWidget::item {
		# 		margin-right: 40px;
		# 	}
        # 	""")
        self.nothing_to_see_label = QLabel("Nothing to see yet...")
        self.nothing_to_see_label.setFont(h2_font)
        self.nothing_to_see_label.setAlignment(Qt.AlignmentFlag.AlignCenter)

        self.sidebar_layout.addWidget(self.db_tree)

        # self.create_demo_tree()
        # self.db_tree.create_demo_tree()


        self.editor_layout.addWidget(self.nothing_to_see_label)
        self.editor_layout.addWidget(self.editor_frame)
        self.editor_layout.addWidget(self.create_page)

        self.layout.addLayout(self.sidebar_layout, 1)
        self.layout.addLayout(self.editor_layout, 3)

        self.show_unselected_state()




    # def create_demo_tree(self):
    # 	rootItem1 = QTreeWidgetItem(['Root Item 1'])
    # 	rootItem2 = QTreeWidgetItem(['Root Item 2'])

    # 	self.db_tree.addTopLevelItem(rootItem1)
    # 	self.db_tree.addTopLevelItem(rootItem2)

    # 	childItem1 = QTreeWidgetItem(['Child Item 1'])
    # 	rootItem1.addChild(childItem1)

    # 	self.db_tree.setCurrentItem(childItem1)

    def load_tree_data(self):
        code, self.db_data = self.repository.get_db_data()
        self.db_tree.create_tree(self.db_data)

    def show_unselected_state(self):
    	self.editor_layout.setCurrentIndex(0)

    def show_selected_state(self, db_name, table_name):
        self.editor_frame.set_selected_db(db_name)
        self.editor_frame.set_selected_table(table_name)
        self.editor_layout.setCurrentIndex(1)

    def show_create_state(self, create_type, db_name):
        self.editor_layout.setCurrentIndex(2)
        self.create_page.set_submit_action(create_type, db_name)

    def show_delete_state(self, delete_type, db_name, table_name):
        print(delete_type, db_name, table_name)
        if delete_type == "db":
            code, msg = self.repository.drop_database(db_name)
            print(code, msg)
            self.show_unselected_state()
            return code
        else:
            code, msg = self.repository.drop_table(db_name, table_name)
            print(code, msg)
            self.show_unselected_state()
            return code
        return 1
        #create delete successful or failed popup

    # de a dbTree latja az editor frame-t is s be tudja alllitani azt is
    # def set_selected_db(self, db):
    #     self.selected_db = db

    # def set_selected_table(self, table):
    #     self.selected_table = table
